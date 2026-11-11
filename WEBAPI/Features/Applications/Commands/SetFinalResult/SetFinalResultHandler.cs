using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Applications.Commands.SetFinalResult;

public sealed class SetFinalResultHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<SetFinalResultHandler> logger)
    : IRequestHandler<SetFinalResultCommand, Result<BatchApplicationDto>>
{
    public async Task<Result<BatchApplicationDto>> Handle(
        SetFinalResultCommand request, CancellationToken ct)
    {
        var application = await db.BatchApplications
            .Include(batchApplication => batchApplication.Candidate)
            .Include(batchApplication => batchApplication.RecruitmentBatch)
            .Include(batchApplication => batchApplication.ApplicationStages)
            .FirstOrDefaultAsync(
                batchApplication => batchApplication.Id == request.ApplicationId, ct);

        if (application is null)
            return Result<BatchApplicationDto>.Failure(
                Error.NotFound("Application was not found."));

        if (application.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule("Closed recruitment applications cannot be changed."));

        if (application.FinalResultStatus is not null)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule("Final result has already been set."));

        if (application.Status.IsFinal())
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var usesBatchStageLifecycle = await db.BatchStages.AnyAsync(stage =>
            stage.RecruitmentBatchId == application.RecruitmentBatchId &&
            stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive, ct);

        if (usesBatchStageLifecycle)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule(
                    "Final results are published when the final batch stage is completed."));

        var activeStages = await db.BatchStages
            .AsNoTracking()
            .Where(stage =>
                stage.RecruitmentBatchId == application.RecruitmentBatchId &&
                stage.IsActive)
            .ToListAsync(ct);

        if (activeStages.Count == 0)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule("No active stages are configured for this batch."));

        var applicationStagesByBatchStage = application.ApplicationStages
            .GroupBy(stage => stage.BatchStageId)
            .ToDictionary(group => group.Key, group => group.First());

        var activeApplicationStages = new List<ApplicationStage>();

        foreach (var activeStage in activeStages)
        {
            if (!applicationStagesByBatchStage.TryGetValue(activeStage.Id, out var applicationStage))
                return Result<BatchApplicationDto>.Failure(
                    Error.BusinessRule(
                        "All active stages must be started before setting the final result."));

            activeApplicationStages.Add(applicationStage);
        }

        if (activeApplicationStages.Any(stage => !stage.Status.IsTerminalStage()))
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule(
                    "All active stages must be completed before setting the final result."));

        if (request.FinalResultStatus == FinalResultStatus.Accepted &&
            activeApplicationStages.Any(stage => !stage.Status.IsPassingStage()))
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule(
                    "The application can only be accepted when all active stages have passed."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        application.FinalResultStatus = request.FinalResultStatus;
        application.Notes = request.Notes.Trim();
        application.UpdatedAt = now;

        if (request.FinalResultStatus == FinalResultStatus.Accepted)
        {
            application.Status = ApplicationStatus.Accepted;
            application.OfferingLetterStatus = OfferingLetterStatus.Pending;
        }
        else
        {
            application.Status = ApplicationStatus.Rejected;
        }

        await db.SaveChangesAsync(ct);

        if (request.FinalResultStatus == FinalResultStatus.Rejected)
            await SendFinalRejectionEmailAsync(application, ct);

        return Result<BatchApplicationDto>.Success(application.ToDto());
    }

    private async Task SendFinalRejectionEmailAsync(
        BatchApplication application, CancellationToken ct)
    {
        try
        {
            var content = EmailTemplates.FinalResultRejection(
                application.Candidate.FullName,
                application.RecruitmentBatch.Name,
                application.Notes);

            await emailService.SendEmailAsync(
                application.Candidate.Email,
                content,
                EmailType.Rejection,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim email hasil akhir untuk application {ApplicationId}",
                application.Id);
        }
    }
}
