using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Applications.Commands.BulkStartNextStage;

public sealed class BulkStartNextStageHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ApplicationStageProgressionService progression,
    IEmailService emailService,
    ILogger<BulkStartNextStageHandler> logger)
    : IRequestHandler<BulkStartNextStageCommand, Result<BulkStartNextStageDto>>
{
    public async Task<Result<BulkStartNextStageDto>> Handle(
        BulkStartNextStageCommand request,
        CancellationToken ct)
    {
        var batch = await db.RecruitmentBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(batch => batch.Id == request.BatchId, ct);

        if (batch is null)
            return Result<BulkStartNextStageDto>.Failure(
                Error.NotFound("Batch was not found."));

        var currentStage = await db.BatchStages
            .AsNoTracking()
            .Include(stage => stage.StageType)
            .FirstOrDefaultAsync(stage =>
                stage.Id == request.StageId &&
                stage.RecruitmentBatchId == request.BatchId, ct);

        if (currentStage is null)
            return Result<BulkStartNextStageDto>.Failure(
                Error.NotFound("Batch stage was not found."));

        if (!currentStage.IsActive)
            return Result<BulkStartNextStageDto>.Failure(
                Error.BusinessRule(
                    $"Tahap '{currentStage.StageName}' tidak aktif sehingga tidak dapat diproses."));

        var currentStageApplications = await db.ApplicationStages
            .Include(applicationStage => applicationStage.BatchApplication)
                .ThenInclude(application => application.Candidate)
            .Include(applicationStage => applicationStage.BatchApplication)
                .ThenInclude(application => application.RecruitmentBatch)
            .Where(applicationStage =>
                applicationStage.BatchStageId == currentStage.Id &&
                applicationStage.BatchApplication.RecruitmentBatchId == request.BatchId)
            .ToListAsync(ct);

        if (currentStageApplications.Count == 0)
            return Result<BulkStartNextStageDto>.Failure(
                Error.BusinessRule(
                    $"Belum ada kandidat yang berada pada tahap '{currentStage.StageName}'."));

        var notEvaluated = currentStageApplications
            .Where(applicationStage => !applicationStage.Status.IsTerminalStage())
            .ToList();

        if (notEvaluated.Count > 0)
        {
            var names = string.Join(
                ", ",
                notEvaluated
                    .Select(applicationStage => applicationStage.BatchApplication.Candidate.FullName)
                    .Take(5));

            var suffix = notEvaluated.Count > 5 ? ", ..." : string.Empty;

            return Result<BulkStartNextStageDto>.Failure(
                Error.BusinessRule(
                    $"Bulk start belum dapat dilakukan. Masih ada {notEvaluated.Count} kandidat pada tahap '{currentStage.StageName}' yang belum dievaluasi: {names}{suffix}."));
        }

        var batchStages = await progression.GetBatchStagesAsync(request.BatchId, ct);
        var stagesById = batchStages.ToDictionary(stage => stage.Id);
        var nextStage = progression.GetNextActiveStage(currentStage, stagesById);

        if (nextStage is null)
            return Result<BulkStartNextStageDto>.Failure(
                Error.BusinessRule(
                    "Tidak ada tahap berikutnya yang aktif. Silakan tetapkan hasil akhir kandidat."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (now > nextStage.EndDate)
            return Result<BulkStartNextStageDto>.Failure(
                Error.BusinessRule(
                    $"Tahap berikutnya '{nextStage.StageName}' sudah berakhir pada {nextStage.EndDate:dd MMM yyyy HH:mm}."));

        var durationTest = await progression.GetDurationTestAsync(
            batch.DepartmentId,
            nextStage,
            ct);

        var existingNextStages = await db.ApplicationStages
            .AsNoTracking()
            .Where(applicationStage =>
                applicationStage.BatchStageId == nextStage.Id &&
                applicationStage.BatchApplication.RecruitmentBatchId == request.BatchId)
            .ToDictionaryAsync(
                applicationStage => applicationStage.BatchApplicationId,
                applicationStage => applicationStage,
                ct);

        var started = new List<StartedCandidate>();
        var candidates = new List<BulkStartNextStageCandidateDto>(currentStageApplications.Count);
        var skippedNotPassed = 0;
        var skippedAlreadyStarted = 0;
        var skippedFinal = 0;

        foreach (var currentApplicationStage in currentStageApplications)
        {
            var application = currentApplicationStage.BatchApplication;
            var candidate = application.Candidate;

            var candidateResult = new BulkStartNextStageCandidateDto
            {
                ApplicationId = application.Id,
                CandidateId = candidate.Id,
                CandidateName = candidate.FullName,
                StageStatus = currentApplicationStage.Status
            };

            if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            {
                skippedFinal++;
                candidateResult.Result = "SkippedFinalized";
                candidateResult.Message = "Application sudah final.";
                candidates.Add(candidateResult);
                continue;
            }

            if (!currentApplicationStage.Status.IsPassingStage())
            {
                skippedNotPassed++;
                candidateResult.Result = "SkippedNotPassed";
                candidateResult.Message = "Kandidat tidak lolos pada tahap ini.";
                candidates.Add(candidateResult);
                continue;
            }

            if (existingNextStages.TryGetValue(application.Id, out var existingNextStage))
            {
                skippedAlreadyStarted++;
                candidateResult.Result = "SkippedAlreadyStarted";
                candidateResult.ApplicationStageId = existingNextStage.Id;
                candidateResult.Message = "Kandidat sudah memulai tahap berikutnya.";
                candidates.Add(candidateResult);
                continue;
            }

            var newApplicationStage = progression.CreateApplicationStage(
                application,
                nextStage,
                now,
                durationTest);

            db.ApplicationStages.Add(newApplicationStage);

            candidateResult.ApplicationStageId = newApplicationStage.Id;
            candidateResult.Result = "Started";

            started.Add(new StartedCandidate(application, newApplicationStage, candidateResult));
            candidates.Add(candidateResult);
        }

        if (started.Count > 0)
        {
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                logger.LogWarning(
                    ex,
                    "Bulk start gagal: sebagian kandidat sudah memulai tahap {StageId} pada batch {BatchId}",
                    nextStage.Id,
                    request.BatchId);

                return Result<BulkStartNextStageDto>.Failure(
                    Error.Conflict(
                        "Sebagian kandidat sudah memulai tahap berikutnya. Silakan muat ulang data."));
            }
        }

        var emailSent = 0;
        var emailFailed = 0;

        if (started.Count > 0 && progression.RequiresStageInvitation(nextStage))
        {
            var messages = started
                .Where(item => !string.IsNullOrWhiteSpace(item.Application.Candidate.Email))
                .Select(item => new BulkEmailMessage(
                    item.Application.Candidate.Email,
                    progression.BuildStageInvitation(
                        item.Application,
                        nextStage,
                        item.ApplicationStage)))
                .ToList();

            var emailResults = new List<EmailSendResult>();

            if (messages.Count > 0)
            {
                try
                {
                    emailResults = (await emailService.SendBulkEmailAsync(
                        messages,
                        EmailType.StageInvitation,
                        ct)).ToList();
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Gagal mengirim email bulk undangan tahap {StageId} untuk batch {BatchId}",
                        nextStage.Id,
                        request.BatchId);
                }
            }

            var emailResultsByRecipient = emailResults
                .GroupBy(result => result.RecipientEmail, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var item in started)
            {
                var email = item.Application.Candidate.Email;

                if (string.IsNullOrWhiteSpace(email))
                {
                    emailFailed++;
                    item.Result.EmailSent = false;
                    item.Result.Message = "Email kandidat kosong.";
                    continue;
                }

                if (emailResultsByRecipient.TryGetValue(email, out var emailResult) &&
                    emailResult.Success)
                {
                    emailSent++;
                    item.Result.EmailSent = true;
                    continue;
                }

                emailFailed++;
                item.Result.EmailSent = false;
                item.Result.Message = emailResult?.ErrorMessage ?? "Gagal mengirim email undangan.";
            }
        }

        return Result<BulkStartNextStageDto>.Success(new BulkStartNextStageDto
        {
            BatchId = request.BatchId,
            CurrentStageId = currentStage.Id,
            CurrentStageName = currentStage.StageName,
            NextStageId = nextStage.Id,
            NextStageName = nextStage.StageName,
            TotalCandidates = currentStageApplications.Count,
            EvaluatedCandidates = currentStageApplications.Count,
            PassedCandidates = currentStageApplications.Count(
                applicationStage => applicationStage.Status.IsPassingStage()),
            StartedCount = started.Count,
            SkippedNotPassedCount = skippedNotPassed,
            SkippedAlreadyStartedCount = skippedAlreadyStarted,
            SkippedFinalCount = skippedFinal,
            EmailSentCount = emailSent,
            EmailFailedCount = emailFailed,
            Candidates = candidates
        });
    }

    private sealed record StartedCandidate(
        BatchApplication Application,
        ApplicationStage ApplicationStage,
        BulkStartNextStageCandidateDto Result);
}
