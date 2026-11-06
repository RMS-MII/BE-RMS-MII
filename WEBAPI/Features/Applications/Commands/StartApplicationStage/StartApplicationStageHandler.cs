using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Applications.Commands.StartApplicationStage;

public sealed class StartApplicationStageHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<StartApplicationStageHandler> logger)
    : IRequestHandler<StartApplicationStageCommand, Result<ApplicationStageDto>>
{
    public async Task<Result<ApplicationStageDto>> Handle(
        StartApplicationStageCommand request, CancellationToken ct)
    {
        var application = await db.BatchApplications
            .Include(application => application.Candidate)
            .Include(application => application.RecruitmentBatch)
            .FirstOrDefaultAsync(application => application.Id == request.ApplicationId, ct);

        if (application is null)
            return Result<ApplicationStageDto>.Failure(
                Error.NotFound("Application was not found."));

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var batchStages = await db.BatchStages
            .AsNoTracking()
            .Where(stage => stage.RecruitmentBatchId == application.RecruitmentBatchId)
            .Include(stage => stage.StageType)
            .ToListAsync(ct);

        var stagesById = batchStages.ToDictionary(stage => stage.Id);

        var activeStages = batchStages
            .Where(stage => stage.IsActive)
            .OrderBy(stage => stage.StartDate)
            .ThenBy(stage => stage.CreatedAt)
            .ToList();

        if (activeStages.Count == 0)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("No active stages are configured for this batch."));

        var applicationStages = await db.ApplicationStages
            .AsNoTracking()
            .Where(stage => stage.BatchApplicationId == application.Id)
            .ToListAsync(ct);

        var activeStageIndexes = activeStages
            .Select((stage, index) => new { stage.Id, Index = index })
            .ToDictionary(item => item.Id, item => item.Index);

        var lastStartedStage = applicationStages
            .Where(stage => activeStageIndexes.ContainsKey(stage.BatchStageId))
            .OrderBy(stage => activeStageIndexes[stage.BatchStageId])
            .LastOrDefault();

        BatchStage nextStage;

        if (lastStartedStage is null)
        {
            nextStage = activeStages[0];
        }
        else
        {
            if (applicationStages.Any(stage => !stage.Status.IsTerminalStage()))
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule(
                        "A stage is still in progress or waiting for evaluation."));

            if (applicationStages.Any(stage =>
                    stage.Status is ApplicationStageStatus.Rejected
                        or ApplicationStageStatus.Dropped
                        or ApplicationStageStatus.Expired))
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule(
                        "The application cannot continue because a stage did not pass."));

            if (!lastStartedStage.Status.IsPassingStage())
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule(
                        "The application cannot continue because the last stage did not pass."));

            if (!stagesById.TryGetValue(lastStartedStage.BatchStageId, out var lastBatchStage))
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule("The last stage is not part of this batch."));

            var nextActiveStage = GetNextActiveStage(lastBatchStage, stagesById);

            if (nextActiveStage is null)
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule(
                        "All active stages have been completed. Set the final result instead."));

            nextStage = nextActiveStage;
        }

        if (nextStage.StageType is null)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("The next stage does not have a stage type."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (now > nextStage.EndDate)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("The stage has already ended."));

        var applicationStage = new ApplicationStage
        {
            Id = Guid.CreateVersion7(),
            BatchApplicationId = application.Id,
            BatchStageId = nextStage.Id,
            Status = ApplicationStageStatus.InProgress,
            StartedAt = now,
            EndAt = nextStage.EndDate,
            CompletedAt = null,
            Notes = null,
            IsAttempts = false,
            DurationTest = null,
            CreatedAt = now
        };

        application.UpdatedAt = now;
        db.ApplicationStages.Add(applicationStage);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            logger.LogWarning(
                "Start stage gagal: tahap {BatchStageId} sudah dimulai untuk application {ApplicationId}",
                nextStage.Id, application.Id);

            return Result<ApplicationStageDto>.Failure(
                Error.Conflict("This stage has already been started for the application."));
        }

        if (nextStage.StageType.StageCategory == StageCategory.Test)
            await SendStageInvitationAsync(application, nextStage, applicationStage, ct);

        return Result<ApplicationStageDto>.Success(applicationStage.ToDto(nextStage));
    }

    private async Task SendStageInvitationAsync(
        BatchApplication application,
        BatchStage batchStage,
        ApplicationStage applicationStage,
        CancellationToken ct)
    {
        try
        {
            var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);

            var portalUrl = $"{frontendUrl}/portal/{application.CandidateId}";

            var content = EmailTemplates.StageInvitation(
                application.Candidate.FullName,
                application.RecruitmentBatch.Name,
                batchStage.StageName,
                portalUrl,
                applicationStage.EndAt);

            await emailService.SendEmailAsync(
                application.Candidate.Email,
                content,
                EmailType.StageInvitation,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim undangan tahap {StageName} untuk application {ApplicationId}",
                batchStage.StageName,
                application.Id);
        }
    }

    private static BatchStage? GetNextActiveStage(
        BatchStage currentStage,
        IReadOnlyDictionary<Guid, BatchStage> stagesById)
    {
        var nextStageId = currentStage.NextStageId;

        while (nextStageId.HasValue)
        {
            if (!stagesById.TryGetValue(nextStageId.Value, out var nextStage))
                return null;

            if (nextStage.IsActive)
                return nextStage;

            nextStageId = nextStage.NextStageId;
        }

        return null;
    }
}
