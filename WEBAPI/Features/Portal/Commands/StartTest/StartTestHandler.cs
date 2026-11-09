using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Commands.StartTest;

public sealed class StartTestHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<StartTestCommand, Result<StartTestResultDto>>
{
    public async Task<Result<StartTestResultDto>> Handle(
        StartTestCommand request, CancellationToken ct)
    {
        var stage = await db.ApplicationStages
            .Include(applicationStage => applicationStage.BatchApplication)
            .Include(applicationStage => applicationStage.BatchStage)
                .ThenInclude(batchStage => batchStage.StageType)
            .FirstOrDefaultAsync(applicationStage =>
                applicationStage.Id == request.StageId &&
                applicationStage.BatchApplication.CandidateId == request.CandidateId, ct);

        if (stage is null)
            return Result<StartTestResultDto>.Failure(
                Error.NotFound("Test stage was not found or has not been started yet."));

        var application = stage.BatchApplication;

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        if (stageType.StageCategory != StageCategory.Test)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("This stage is not an online test."));

        if (stage.Status != ApplicationStageStatus.InProgress)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("This test can no longer be started."));

        if (stage.IsAttempts)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("This test has already been attempted."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        // Idempotent: jika kandidat sudah pernah memulai (mis. refresh atau
        // double-click), kembalikan waktu mulai yang sama.
        if (stage.AttemptStartedAt is not null)
        {
            return Result<StartTestResultDto>.Success(new StartTestResultDto
            {
                ApplicationStageId = stage.Id,
                AttemptStartedAt = stage.AttemptStartedAt.Value,
                Deadline = TestAttemptPolicy.GetDeadline(stage),
                ServerTimeUtc = now
            });
        }

        if (now < stage.StartedAt)
            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule("This test has not started yet."));

        if (now > stage.EndAt)
        {
            stage.Status = ApplicationStageStatus.Expired;
            stage.TotalScore = null;
            stage.CompletedAt = now;
            stage.UpdatedAt = now;
            application.UpdatedAt = now;

            await db.SaveChangesAsync(ct);

            return Result<StartTestResultDto>.Failure(
                Error.BusinessRule(
                    "The test access window has ended. The stage has been marked as expired."));
        }

        stage.AttemptStartedAt = now;
        stage.UpdatedAt = now;
        application.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return Result<StartTestResultDto>.Success(new StartTestResultDto
        {
            ApplicationStageId = stage.Id,
            AttemptStartedAt = stage.AttemptStartedAt.Value,
            Deadline = TestAttemptPolicy.GetDeadline(stage),
            ServerTimeUtc = now
        });
    }
}
