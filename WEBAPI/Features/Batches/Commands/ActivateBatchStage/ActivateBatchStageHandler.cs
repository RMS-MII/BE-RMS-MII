using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Batches;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Batches.Commands.ActivateBatchStage;

public sealed class ActivateBatchStageHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<ActivateBatchStageHandler> logger)
    : IRequestHandler<ActivateBatchStageCommand, Result<BatchStageTransitionDto>>
{
    public async Task<Result<BatchStageTransitionDto>> Handle(
        ActivateBatchStageCommand request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var batch = await db.RecruitmentBatches
            .FirstOrDefaultAsync(item => item.Id == request.BatchId, ct);

        if (batch is null)
            return Result<BatchStageTransitionDto>.Failure(Error.NotFound("Batch was not found."));

        if (batch.Status != BatchStatus.Open)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Open the recruitment batch before activating a stage."));

        var hasCandidates = await db.BatchApplications
            .AsNoTracking()
            .AnyAsync(application => application.RecruitmentBatchId == batch.Id && !application.IsStandalone, ct);

        if (!hasCandidates)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The recruitment batch has no candidates yet."));

        var stages = await db.BatchStages
            .Where(stage => stage.RecruitmentBatchId == request.BatchId)
            .OrderBy(stage => stage.StartDate)
            .ThenBy(stage => stage.CreatedAt)
            .ThenBy(stage => stage.Id)
            .Include(stage => stage.StageType)
            .ToListAsync(ct);

        var targetStage = stages.FirstOrDefault(stage => stage.Id == request.StageId);
        if (targetStage is null)
            return Result<BatchStageTransitionDto>.Failure(
                Error.NotFound("Stage was not found in this batch."));

        if (targetStage.LifecycleStatus == BatchStageLifecycleStatus.Active)
        {
            await transaction.CommitAsync(ct);
            var existingCount = await db.ApplicationStages
                .CountAsync(item => item.BatchStageId == targetStage.Id && !item.BatchApplication.IsStandalone, ct);
            return Result<BatchStageTransitionDto>.Success(new BatchStageTransitionDto
            {
                BatchStageId = targetStage.Id,
                LifecycleStatus = targetStage.LifecycleStatus,
                CandidatesStarted = existingCount,
                AlreadyApplied = true
            });
        }

        if (targetStage.LifecycleStatus == BatchStageLifecycleStatus.Completed)
            return Result<BatchStageTransitionDto>.Failure(
                Error.Conflict("A completed stage cannot be activated again."));

        if (targetStage.StageType.StageCategory == StageCategory.Interview)
        {
            var interviewers = await db.StageEmployees
                .Where(stageEmployee => stageEmployee.BatchStageId == targetStage.Id)
                .ToListAsync(ct);

            var acceptedInterviewers = interviewers
                .Where(stageEmployee => stageEmployee.Status == StageEmployeeStatus.Accepted)
                .ToList();

            if (acceptedInterviewers.Count == 0)
                return Result<BatchStageTransitionDto>.Failure(
                    Error.BusinessRule(
                        $"The interview stage '{targetStage.StageName}' cannot start yet because no interviewer has accepted the assignment (Accepted)."));
        }

        var orderedStages = BatchStageSequence.GetOrderedStages(stages);
        if (orderedStages.Count != stages.Count)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The configured stage sequence is invalid. Reconfigure the batch stages."));

        var configuredStages = orderedStages;
        if (configuredStages.Count == 0)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Configure at least one stage before activating the batch."));

        var currentStage = stages.SingleOrDefault(
            stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active);
        var expectedStage = currentStage is null
            ? configuredStages[0]
            : GetNextConfiguredStage(currentStage, configuredStages);

        if (expectedStage is null)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The final stage is active. Complete it instead of activating another stage."));

        if (expectedStage.Id != targetStage.Id)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule($"Activate the next stage in order: {expectedStage.StageName}."));

        if (currentStage is not null &&
            stages.Any(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active &&
                                stage.Id != currentStage.Id))
            return Result<BatchStageTransitionDto>.Failure(
                Error.Conflict("Another stage is already active for this batch."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        if (currentStage is null && batch.ApplicationDeadlineAt > now)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Wait until the application deadline before starting the first batch stage."));

        if (now > targetStage.EndDate)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The stage end date has passed."));

        var isTestStage = targetStage.StageType.StageCategory == StageCategory.Test;
        var startedAt = now;

        var applications = await db.BatchApplications
            .Where(application => application.RecruitmentBatchId == request.BatchId && !application.IsStandalone)
            .Include(application => application.Candidate)
            .Include(application => application.ApplicationStages)
            .ToListAsync(ct);

        var candidatesStarted = 0;
        var candidatesPassed = 0;
        var candidatesRejected = 0;
        var candidatesDropped = 0;
        var candidatesExpired = 0;
        var startedApplications = new List<(BatchApplication Application, ApplicationStage Stage)>();
        var rejectedApplications = new List<(BatchApplication Application, ApplicationStage Stage)>();

        if (currentStage is null)
        {
            var legacyStageRecords = applications
                .SelectMany(application => application.ApplicationStages)
                .Where(applicationStage => applicationStage.BatchStageId != targetStage.Id)
                .ToList();

            if (legacyStageRecords.Count > 0)
                return Result<BatchStageTransitionDto>.Failure(
                    Error.BusinessRule(
                        "Candidate stages have already been started with the per-candidate flow. Resolve or migrate those stages before batch activation."));

            var eligibleApplications = applications
                .Where(application =>
                    application.Status == ApplicationStatus.InProcess &&
                    application.FinalResultStatus is null)
                .ToList();

            if (eligibleApplications.Count == 0)
                return Result<BatchStageTransitionDto>.Failure(
                    Error.BusinessRule("Register at least one eligible candidate before starting the first stage."));

            var durationTest = await GetTestDurationAsync(batch.DepartmentId, targetStage, ct);

            foreach (var application in eligibleApplications)
            {
                var existing = application.ApplicationStages
                    .FirstOrDefault(item => item.BatchStageId == targetStage.Id);

                if (existing is null)
                {
                    existing = CreateApplicationStage(application.Id, targetStage, startedAt, now, durationTest);
                    db.ApplicationStages.Add(existing);
                }

                startedApplications.Add((application, existing));
                candidatesStarted++;
            }
        }
        else
        {
            var currentParticipants = applications
                .SelectMany(application => application.ApplicationStages
                    .Where(item => item.BatchStageId == currentStage.Id)
                    .Select(item => new { Application = application, Stage = item }))
                .ToList();

            var participantApplicationIds = currentParticipants
                .Select(item => item.Application.Id)
                .ToHashSet();
            var missingStageApplications = applications.Any(application =>
                application.Status == ApplicationStatus.InProcess &&
                application.FinalResultStatus is null &&
                !participantApplicationIds.Contains(application.Id));

            if (missingStageApplications)
                return Result<BatchStageTransitionDto>.Failure(
                    Error.BusinessRule(
                        "Every active candidate must be assigned to the current stage before advancing."));

            if (currentParticipants.Any(item => !item.Stage.Status.IsTerminalStage()) && !isTestStage)
                return Result<BatchStageTransitionDto>.Failure(
                    Error.BusinessRule(
                        "Evaluate every candidate in the current stage before activating the next stage."));

            foreach (var item in currentParticipants)
            {
                switch (item.Stage.Status)
                {
                    case ApplicationStageStatus.Prioritized:
                    case ApplicationStageStatus.Considered:
                        candidatesPassed++;
                        break;
                    case ApplicationStageStatus.Rejected:
                        item.Application.Status = ApplicationStatus.Rejected;
                        item.Application.UpdatedAt = now;
                        rejectedApplications.Add((item.Application, item.Stage));
                        candidatesRejected++;
                        break;
                    case ApplicationStageStatus.Dropped:
                        item.Application.Status = ApplicationStatus.Dropped;
                        item.Application.UpdatedAt = now;
                        rejectedApplications.Add((item.Application, item.Stage));
                        candidatesDropped++;
                        break;
                    case ApplicationStageStatus.InProgress:
                        item.Stage.Status = ApplicationStageStatus.Dropped;
                        item.Stage.CompletedAt = now;
                        item.Stage.UpdatedAt = now;
                        item.Application.Status = ApplicationStatus.Dropped;
                        item.Application.UpdatedAt = now;
                        rejectedApplications.Add((item.Application, item.Stage));
                        candidatesDropped++;
                        break;
                    case ApplicationStageStatus.Expired:
                        item.Application.Status = ApplicationStatus.Dropped;
                        item.Application.UpdatedAt = now;
                        rejectedApplications.Add((item.Application, item.Stage));
                        candidatesExpired++;
                        break;
                }
            }

            var passingApplications = currentParticipants
                .Where(item => item.Stage.Status.IsPassingStage() &&
                               item.Application.Status == ApplicationStatus.InProcess &&
                               item.Application.FinalResultStatus is null)
                .Select(item => item.Application)
                .ToList();

            var durationTest = await GetTestDurationAsync(batch.DepartmentId, targetStage, ct);

            foreach (var application in passingApplications)
            {
                var existing = application.ApplicationStages
                    .FirstOrDefault(item => item.BatchStageId == targetStage.Id);

                if (existing is null)
                {
                    existing = CreateApplicationStage(
                        application.Id, targetStage, startedAt, now, durationTest);
                    db.ApplicationStages.Add(existing);
                }

                startedApplications.Add((application, existing));
                candidatesStarted++;
            }

            currentStage.PlannedDurationTicks ??= (currentStage.EndDate - currentStage.StartDate).Ticks;
            if (currentStage.StartDate > now)
                currentStage.StartDate = now;
            if (currentStage.EndDate > now)
                currentStage.EndDate = now;

            foreach (var item in currentParticipants.Where(item => item.Stage.EndAt > now))
            {
                if (item.Stage.StartedAt > now)
                    item.Stage.StartedAt = now;
                item.Stage.EndAt = now;
                item.Stage.UpdatedAt = now;
            }

            currentStage.LifecycleStatus = BatchStageLifecycleStatus.Completed;
            currentStage.UpdatedAt = now;
        }

        foreach (var (_, stage) in startedApplications)
        {
            if (stage.Status == ApplicationStageStatus.InProgress &&
                stage.AttemptStartedAt is null && !stage.IsAttempts && stage.StartedAt > now)
            {
                stage.StartedAt = now;
                stage.UpdatedAt = now;
            }
        }

        if (currentStage is not null)
        {
            var passedMessages = startedApplications
                .Where(item => !string.IsNullOrWhiteSpace(item.Application.Candidate.Email))
                .Select(item => new BulkEmailMessage(
                    item.Application.Candidate.Email,
                    EmailTemplates.StagePassed(
                        item.Application.Candidate.FullName,
                        currentStage.StageName,
                        targetStage.StageName,
                        batch.Name,
                        EmailTemplates.GetCandidatePortalUrl(configuration, item.Application.CandidateId),
                        isTestStage ? item.Stage.EndAt : null)))
                .ToList();
            emailService.QueueBulkEmail(passedMessages, EmailType.StageResult);
        }
        else if (isTestStage)
        {
            var invitations = startedApplications
                .Where(item => !string.IsNullOrWhiteSpace(item.Application.Candidate.Email))
                .Select(item => new BulkEmailMessage(
                    item.Application.Candidate.Email,
                    EmailTemplates.StageInvitation(
                        item.Application.Candidate.FullName,
                        batch.Name,
                        targetStage.StageName,
                        EmailTemplates.GetCandidatePortalUrl(configuration, item.Application.CandidateId),
                        item.Stage.EndAt)))
                .ToList();
            emailService.QueueBulkEmail(invitations, EmailType.StageInvitation);
        }

        var rejections = rejectedApplications
            .Where(item => !string.IsNullOrWhiteSpace(item.Application.Candidate.Email))
            .Select(item => new BulkEmailMessage(
                item.Application.Candidate.Email,
                EmailTemplates.StageRejection(
                    item.Application.Candidate.FullName,
                    currentStage!.StageName,
                    batch.Name,
                    item.Stage.Notes)))
            .ToList();
        emailService.QueueBulkEmail(rejections, EmailType.Rejection);

        try
        {
            // The filtered unique index permits only one Active stage per batch.
            // Persist the old stage as Completed before activating the next one.
            if (currentStage is not null)
                await db.SaveChangesAsync(ct);

            if (targetStage.StartDate != startedAt)
            {
                targetStage.PlannedDurationTicks ??= (targetStage.EndDate - targetStage.StartDate).Ticks;
                targetStage.StartDate = startedAt;
            }

            targetStage.LifecycleStatus = BatchStageLifecycleStatus.Active;
            targetStage.UpdatedAt = now;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning(
                ex,
                "Stage activation conflict for batch {BatchId}, stage {StageId}",
                request.BatchId,
                request.StageId);
            return Result<BatchStageTransitionDto>.Failure(
                Error.Conflict("The batch stage changed concurrently. Refresh and try again."));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            var affectedEntities = string.Join(", ", ex.Entries.Select(entry =>
                $"{entry.Metadata.ClrType.Name}:{entry.Properties.FirstOrDefault(property => property.Metadata.IsPrimaryKey())?.CurrentValue}"));
            logger.LogWarning(ex,
                "Stage activation could not save {AffectedEntities} for batch {BatchId}",
                affectedEntities, request.BatchId);
            return Result<BatchStageTransitionDto>.Failure(
                Error.Conflict("The batch stage changed concurrently. Refresh and try again."));
        }

        return Result<BatchStageTransitionDto>.Success(new BatchStageTransitionDto
        {
            BatchStageId = targetStage.Id,
            LifecycleStatus = targetStage.LifecycleStatus,
            CandidatesStarted = candidatesStarted,
            CandidatesPassed = candidatesPassed,
            CandidatesRejected = candidatesRejected,
            CandidatesDropped = candidatesDropped,
            CandidatesExpired = candidatesExpired
        });
    }

    private static BatchStage? GetNextConfiguredStage(
        BatchStage currentStage,
        IReadOnlyCollection<BatchStage> configuredStages)
    {
        var ordered = configuredStages.ToList();
        var currentIndex = ordered.FindIndex(stage => stage.Id == currentStage.Id);
        return currentIndex >= 0 && currentIndex + 1 < ordered.Count
            ? ordered[currentIndex + 1]
            : null;
    }

    private async Task<TimeSpan?> GetTestDurationAsync(
        Guid departmentId,
        BatchStage stage,
        CancellationToken ct)
    {
        if (stage.StageType?.StageCategory != StageCategory.Test)
            return null;

        var durationInMinutes = await db.QuestionBanks
            .AsNoTracking()
            .Where(questionBank =>
                questionBank.DepartmentId == departmentId &&
                questionBank.StageTypeId == stage.StageTypeId)
            .OrderBy(questionBank => questionBank.CreatedAt)
            .ThenBy(questionBank => questionBank.Id)
            .Select(questionBank => (int?)questionBank.DurationInMinutes)
            .FirstOrDefaultAsync(ct);

        return durationInMinutes.HasValue
            ? TimeSpan.FromMinutes(durationInMinutes.Value)
            : null;
    }

    private static ApplicationStage CreateApplicationStage(
        Guid applicationId,
        BatchStage batchStage,
        DateTime startedAt,
        DateTime now,
        TimeSpan? durationTest) => new()
    {
        Id = Guid.CreateVersion7(),
        BatchApplicationId = applicationId,
        BatchStageId = batchStage.Id,
        Status = ApplicationStageStatus.InProgress,
        StartedAt = startedAt,
        EndAt = batchStage.EndDate,
        CompletedAt = null,
        Notes = null,
        IsAttempts = false,
        DurationTest = durationTest,
        CreatedAt = now
    };
}
