using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;

public sealed class EvaluateStageHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<EvaluateStageCommand, Result<ApplicationStageDto>>
{
    public async Task<Result<ApplicationStageDto>> Handle(
        EvaluateStageCommand request, CancellationToken ct)
    {
        var stage = await db.ApplicationStages
            .Include(applicationStage => applicationStage.BatchApplication)
            .Include(applicationStage => applicationStage.BatchStage)
                .ThenInclude(batchStage => batchStage.StageType)
            .FirstOrDefaultAsync(applicationStage => applicationStage.Id == request.StageId, ct);

        if (stage is null)
            return Result<ApplicationStageDto>.Failure(
                Error.NotFound("Application stage was not found."));

        var application = stage.BatchApplication;

        var usesBatchStageLifecycle = await db.BatchStages.AnyAsync(batchStage =>
            batchStage.RecruitmentBatchId == application.RecruitmentBatchId &&
            batchStage.LifecycleStatus != BatchStageLifecycleStatus.Inactive, ct);

        if (usesBatchStageLifecycle &&
            stage.BatchStage.LifecycleStatus != BatchStageLifecycleStatus.Active)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("Only candidates in the active batch stage can be evaluated."));

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        var canConvertExpiredTestToDropped =
            request.Status == ApplicationStageStatus.Dropped &&
            stage.Status == ApplicationStageStatus.Expired &&
            stageType.StageCategory == StageCategory.Test;

        if (stage.Status.IsTerminalStage() && !canConvertExpiredTestToDropped)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("Stage has already been evaluated."));

        if (request.Status == ApplicationStageStatus.Dropped &&
            stageType.StageCategory == StageCategory.Test &&
            stage.Status == ApplicationStageStatus.Submitted)
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("A submitted test cannot be marked as dropped."));

        if (stageType.StageCategory == StageCategory.Test)
        {
            if (request.Status != ApplicationStageStatus.Dropped &&
                stage.Status != ApplicationStageStatus.Submitted)
                return Result<ApplicationStageDto>.Failure(
                    Error.BusinessRule(
                        "Test stage must be submitted before it can be evaluated."));
        }
        else if (stageType.StageCategory == StageCategory.Interview)
        {
            var interviewValidation = request.Status == ApplicationStageStatus.Dropped
                ? null
                : await ValidateInterviewAssessmentsAsync(stage.Id, ct);

            if (interviewValidation is not null)
                return Result<ApplicationStageDto>.Failure(interviewValidation);
        }
        else
        {
            return Result<ApplicationStageDto>.Failure(
                Error.BusinessRule("The stage category is not supported."));
        }

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        stage.Status = request.Status;
        stage.Notes = request.Notes.Trim();
        stage.CompletedAt = now;
        stage.UpdatedAt = now;
        application.UpdatedAt = now;

        if (request.Status == ApplicationStageStatus.Rejected)
            application.Status = ApplicationStatus.Rejected;
        else if (request.Status == ApplicationStageStatus.Dropped)
            application.Status = ApplicationStatus.Dropped;

        await db.SaveChangesAsync(ct);

        return Result<ApplicationStageDto>.Success(stage.ToDto(stage.BatchStage));
    }

    private async Task<Error?> ValidateInterviewAssessmentsAsync(
        Guid applicationStageId,
        CancellationToken ct)
    {
        var sessions = await db.InterviewSessions
            .AsNoTracking()
            .Where(session =>
                session.ApplicationStageId == applicationStageId &&
                session.Status != InterviewSessionStatus.Rescheduled)
            .Include(session => session.SessionInterviews)
                .ThenInclude(sessionInterview => sessionInterview.InterviewAssessment)
            .ToListAsync(ct);

        var activeSessionInterviews = sessions
            .SelectMany(session => session.SessionInterviews)
            .Where(sessionInterview =>
                sessionInterview.Status != SessionInterviewStatus.Cancelled)
            .ToList();

        if (sessions.Count == 0 || activeSessionInterviews.Count == 0)
            return Error.BusinessRule(
                "Interview sessions or interviewers have not been scheduled.");

        if (activeSessionInterviews.Any(sessionInterview =>
                sessionInterview.InterviewAssessment is null))
            return Error.BusinessRule(
                "All interviewers must submit their assessments before the stage can be evaluated.");

        return null;
    }
}
