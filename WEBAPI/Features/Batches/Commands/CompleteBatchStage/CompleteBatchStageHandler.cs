using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Batches;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Batches.Commands.CompleteBatchStage;

public sealed class CompleteBatchStageHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService)
    : IRequestHandler<CompleteBatchStageCommand, Result<BatchStageTransitionDto>>
{
    public async Task<Result<BatchStageTransitionDto>> Handle(
        CompleteBatchStageCommand request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var batch = await db.RecruitmentBatches
            .FirstOrDefaultAsync(item => item.Id == request.BatchId, ct);

        if (batch is null)
            return Result<BatchStageTransitionDto>.Failure(Error.NotFound("Batch was not found."));

        if (batch.Status == BatchStatus.Draft)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The recruitment batch must be opened before completing its final stage."));

        var stages = await db.BatchStages
            .Where(stage => stage.RecruitmentBatchId == request.BatchId)
            .OrderBy(stage => stage.StartDate)
            .ThenBy(stage => stage.CreatedAt)
            .ThenBy(stage => stage.Id)
            .ToListAsync(ct);

        var targetStage = stages.FirstOrDefault(stage => stage.Id == request.StageId);
        if (targetStage is null)
            return Result<BatchStageTransitionDto>.Failure(
                Error.NotFound("Stage was not found in this batch."));

        var participants = await db.ApplicationStages
            .Where(stage => stage.BatchStageId == targetStage.Id)
            .Include(stage => stage.BatchApplication)
                .ThenInclude(application => application.Candidate)
            .ToListAsync(ct);

        if (targetStage.LifecycleStatus == BatchStageLifecycleStatus.Completed)
        {
            await transaction.CommitAsync(ct);
            return Result<BatchStageTransitionDto>.Success(BuildCompletedResult(targetStage.Id, participants, true));
        }

        if (targetStage.LifecycleStatus != BatchStageLifecycleStatus.Active)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Only the active stage can be completed."));

        var orderedStages = BatchStageSequence.GetOrderedStages(stages);
        if (orderedStages.Count != stages.Count)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("The configured stage sequence is invalid. Reconfigure the batch stages."));

        var configuredStages = orderedStages.Where(stage => stage.IsActive).ToList();
        if (configuredStages.Count == 0 || configuredStages[^1].Id != targetStage.Id)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Only the final configured stage can be completed."));

        var participantIds = participants
            .Select(item => item.BatchApplicationId)
            .ToHashSet();
        var missingApplications = await db.BatchApplications
            .AnyAsync(application =>
                application.RecruitmentBatchId == request.BatchId &&
                application.Status == ApplicationStatus.InProcess &&
                application.FinalResultStatus == null &&
                !participantIds.Contains(application.Id), ct);

        if (missingApplications)
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Every active candidate must be assigned to the final stage before completing it."));

        if (participants.Any(item => !item.Status.IsTerminalStage()))
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Evaluate every candidate in the final stage before completing it."));

        if (participants.Any(item =>
                item.Status is not (ApplicationStageStatus.Prioritized or
                    ApplicationStageStatus.Considered or
                    ApplicationStageStatus.Rejected or
                    ApplicationStageStatus.Dropped or
                    ApplicationStageStatus.Expired)))
            return Result<BatchStageTransitionDto>.Failure(
                Error.BusinessRule("Every candidate must have a publishable final-stage result."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        foreach (var participant in participants)
        {
            var application = participant.BatchApplication;
            application.UpdatedAt = now;

            if (participant.Status.IsPassingStage())
            {
                application.Status = ApplicationStatus.Accepted;
                application.FinalResultStatus = FinalResultStatus.Accepted;
                application.OfferingLetterStatus = OfferingLetterStatus.Pending;
            }
            else if (participant.Status == ApplicationStageStatus.Rejected)
            {
                application.Status = ApplicationStatus.Rejected;
                application.FinalResultStatus = FinalResultStatus.Rejected;
            }
            else
            {
                application.Status = ApplicationStatus.Dropped;
            }
        }

        targetStage.LifecycleStatus = BatchStageLifecycleStatus.Completed;
        targetStage.UpdatedAt = now;

        var rejections = participants
            .Where(item => !item.Status.IsPassingStage() &&
                !string.IsNullOrWhiteSpace(item.BatchApplication.Candidate.Email))
            .Select(item => new BulkEmailMessage(
                item.BatchApplication.Candidate.Email,
                EmailTemplates.FinalResultRejection(
                    item.BatchApplication.Candidate.FullName,
                    batch.Name,
                    item.Notes)))
            .ToList();
        emailService.QueueBulkEmail(rejections, EmailType.Rejection);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<BatchStageTransitionDto>.Success(BuildCompletedResult(targetStage.Id, participants, false));
    }

    private static BatchStageTransitionDto BuildCompletedResult(
        Guid stageId,
        IReadOnlyCollection<WEBAPI.Entities.ApplicationStage> participants,
        bool alreadyApplied)
    {
        var passed = participants.Count(item => item.Status.IsPassingStage());
        var rejected = participants.Count(item => item.Status == ApplicationStageStatus.Rejected);
        var dropped = participants.Count(item => item.Status == ApplicationStageStatus.Dropped);
        var expired = participants.Count(item => item.Status == ApplicationStageStatus.Expired);

        return new BatchStageTransitionDto
        {
            BatchStageId = stageId,
            LifecycleStatus = BatchStageLifecycleStatus.Completed,
            CandidatesPassed = passed,
            CandidatesRejected = rejected,
            CandidatesDropped = dropped,
            CandidatesExpired = expired,
            AlreadyApplied = alreadyApplied
        };
    }
}
