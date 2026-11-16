using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Batches.Commands.UpdateBatchStatus;

public sealed class UpdateBatchStatusHandler(RmsDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateBatchStatusCommand, Result<RecruitmentBatchDto>>
{
    public async Task<Result<RecruitmentBatchDto>> Handle(
        UpdateBatchStatusCommand request, CancellationToken ct)
    {
        var batch = await db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .Include(b => b.BatchStages)
                .ThenInclude(s => s.StageType)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, ct);

        if (batch is null)
            return Result<RecruitmentBatchDto>.Failure(Error.NotFound("Batch was not found."));

        if (!batch.Status.CanTransitionTo(request.Status))
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                $"Cannot change recruitment batch status from {batch.Status} to {request.Status}. " +
                "Allowed transitions are Draft to Open and Open to Closed."));

        if (request.Status == BatchStatus.Open && !batch.BatchStages.Any())
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                "Configure at least one selection stage before opening this recruitment batch."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        var deadline = request.ApplicationDeadlineAt?.UtcDateTime;
        var setsDeadline = deadline.HasValue && deadline != batch.ApplicationDeadlineAt;

        if (batch.Status == BatchStatus.Draft && request.Status == BatchStatus.Open && !deadline.HasValue)
            return Result<RecruitmentBatchDto>.Failure(Error.Validation(
                "Set an application deadline before opening recruitment."));

        if (setsDeadline)
        {
            if (request.Status != BatchStatus.Open ||
                batch.BatchStages.Any(stage => stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive))
                return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                    "An application deadline can only be changed before bulk selection starts."));

            if (deadline <= now || deadline > batch.EndDate)
                return Result<RecruitmentBatchDto>.Failure(Error.Validation(
                    "The application deadline must be in the future and on or before the batch end."));

            var stageEndingTooEarly = batch.BatchStages.FirstOrDefault(stage =>
                stage.EndDate <= deadline);
            if (stageEndingTooEarly is not null)
                return Result<RecruitmentBatchDto>.Failure(Error.Validation(
                    $"The application deadline must be before '{stageEndingTooEarly.StageName}' ends."));
        }

        if (request.Status == BatchStatus.Closed &&
            (!batch.BatchStages.Any() ||
             batch.BatchStages.Any(stage => stage.LifecycleStatus != BatchStageLifecycleStatus.Completed)))
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                "Complete all selection stages before closing this recruitment batch."));

        if (request.Status == BatchStatus.Closed && await db.BatchApplications.AnyAsync(application =>
                application.RecruitmentBatchId == batch.Id && application.IsStandalone &&
                application.Status == ApplicationStatus.InProcess, ct))
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                "Finish standalone applications before closing this recruitment batch."));

        if (batch.Status != request.Status || setsDeadline)
        {
            // Recheck the current status and stage requirement in the update itself.
            var updated = await db.RecruitmentBatches
                .Where(b => b.Id == request.BatchId && b.Status == batch.Status)
                .Where(b => request.Status != BatchStatus.Open || b.BatchStages.Any())
                .Where(b => !setsDeadline ||
                    (b.ApplicationDeadlineAt == batch.ApplicationDeadlineAt && deadline <= b.EndDate &&
                     !b.BatchStages.Any(stage => stage.EndDate <= deadline) &&
                     !b.BatchStages.Any(stage => stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive)))
                .Where(b => request.Status != BatchStatus.Closed ||
                    (b.BatchStages.Any() &&
                     !b.BatchStages.Any(stage => stage.LifecycleStatus != BatchStageLifecycleStatus.Completed) &&
                     !b.BatchApplications.Any(application => application.IsStandalone &&
                         application.Status == ApplicationStatus.InProcess)))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Status, request.Status)
                    .SetProperty(b => b.ApplicationDeadlineAt, b => setsDeadline ? deadline : b.ApplicationDeadlineAt)
                    .SetProperty(b => b.UpdatedAt, now), ct);

            if (updated == 0)
                return Result<RecruitmentBatchDto>.Failure(Error.Conflict(
                    "The recruitment batch changed while updating its status. Refresh the batch and try again."));

            batch.Status = request.Status;
            if (setsDeadline)
                batch.ApplicationDeadlineAt = deadline;
            batch.UpdatedAt = now;
        }

        var dto = batch.ToDto();
        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
