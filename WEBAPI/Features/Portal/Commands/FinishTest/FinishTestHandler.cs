using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Commands.FinishTest;

public sealed class FinishTestHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ITestAttemptFinalizer finalizer)
    : IRequestHandler<FinishTestCommand, Result<FinishTestResultDto>>
{
    public async Task<Result<FinishTestResultDto>> Handle(
        FinishTestCommand request, CancellationToken ct)
    {
        var stage = await db.ApplicationStages
            .Include(applicationStage => applicationStage.BatchApplication)
                .ThenInclude(application => application.Candidate)
            .Include(applicationStage => applicationStage.BatchApplication)
                .ThenInclude(application => application.RecruitmentBatch)
            .Include(applicationStage => applicationStage.BatchStage)
                .ThenInclude(batchStage => batchStage.StageType)
            .FirstOrDefaultAsync(applicationStage =>
                applicationStage.Id == request.StageId &&
                applicationStage.BatchApplication.CandidateId == request.CandidateId, ct);

        if (stage is null)
            return Result<FinishTestResultDto>.Failure(
                Error.NotFound("Test stage was not found or has not been started yet."));

        if (stage.BatchStage.StageType?.StageCategory == StageCategory.Test &&
            (stage.IsAttempts || stage.Status == ApplicationStageStatus.Submitted))
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("This test has already been submitted."));

        var application = stage.BatchApplication;

        if (application.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("This recruitment batch has closed."));

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        if (stageType.StageCategory != StageCategory.Test)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("This stage is not an online test."));

        switch (stage.Status)
        {
            case ApplicationStageStatus.Expired:
                return Result<FinishTestResultDto>.Failure(
                    Error.BusinessRule("This test has expired."));
            case ApplicationStageStatus.InProgress:
                break;
            default:
                return Result<FinishTestResultDto>.Failure(
                    Error.BusinessRule("This test can no longer be finished."));
        }

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (stage.AttemptStartedAt is null)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("This test has not been started yet."));

        if (now < stage.StartedAt)
            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule("This test has not started yet."));

        if (TestAttemptPolicy.IsPastDeadline(stage, now))
        {
            stage.Status = ApplicationStageStatus.Expired;
            stage.CompletedAt = now;
            stage.UpdatedAt = now;
            application.UpdatedAt = now;

            await db.SaveChangesAsync(ct);

            await finalizer.FinalizeAsync(stage, now, ct);

            return Result<FinishTestResultDto>.Failure(
                Error.BusinessRule(
                    "The test duration has ended. The stage has been marked as expired."));
        }

        var status = await finalizer.FinalizeAsync(stage, now, ct);

        var answers = await db.CandidateAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationStageId == stage.Id)
            .ToListAsync(ct);

        return Result<FinishTestResultDto>.Success(new FinishTestResultDto
        {
            ApplicationStageId = stage.Id,
            BatchStageId = stage.BatchStageId,
            StageName = stage.BatchStage.StageName,
            Status = status,
            CompletedAt = stage.CompletedAt,
            TotalScore = stage.TotalScore,
            DiscSummaries = DiscSummaryCalculator.Summarize(answers),
            ServerTimeUtc = now
        });
    }
}
