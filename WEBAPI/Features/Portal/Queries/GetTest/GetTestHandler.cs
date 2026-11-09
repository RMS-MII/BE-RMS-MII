using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Queries.GetTest;

public sealed class GetTestHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetTestQuery, Result<CandidateTestDto>>
{
    public async Task<Result<CandidateTestDto>> Handle(
        GetTestQuery request, CancellationToken ct)
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
            return Result<CandidateTestDto>.Failure(
                Error.NotFound("Test stage was not found or has not been started yet."));

        var application = stage.BatchApplication;

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        if (stageType.StageCategory != StageCategory.Test)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("This stage is not an online test."));

        switch (stage.Status)
        {
            case ApplicationStageStatus.Submitted:
                return Result<CandidateTestDto>.Failure(
                    Error.BusinessRule("This test has already been submitted."));
            case ApplicationStageStatus.Expired:
                return Result<CandidateTestDto>.Failure(
                    Error.BusinessRule("This test has expired."));
            case ApplicationStageStatus.InProgress:
                break;
            default:
                return Result<CandidateTestDto>.Failure(
                    Error.BusinessRule("This test can no longer be taken."));
        }

        if (stage.IsAttempts)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("This test has already been attempted."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (now < stage.StartedAt)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("This test has not started yet."));

        if (now > stage.EndAt)
        {
            stage.Status = ApplicationStageStatus.Expired;
            stage.TotalScore = null;
            stage.CompletedAt = now;
            stage.UpdatedAt = now;
            application.UpdatedAt = now;

            await db.SaveChangesAsync(ct);

            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule(
                    "The test deadline has passed. The stage has been marked as expired."));
        }

        var questions = await db.Questions
            .AsNoTracking()
            .Where(question =>
                question.IsActive &&
                question.QuestionBank.DepartmentId == application.RecruitmentBatch.DepartmentId &&
                question.QuestionBank.StageTypeId == stage.BatchStage.StageTypeId)
            .Include(question => question.QuestionOptions.Where(option => option.IsActive))
            .OrderBy(question => question.SequenceNumber)
            .ThenBy(question => question.Id)
            .ToListAsync(ct);

        if (questions.Count == 0)
            return Result<CandidateTestDto>.Failure(
                Error.BusinessRule("No active questions are configured for this test."));

        return Result<CandidateTestDto>.Success(
            stage.ToCandidateTestDto(questions, now));
    }
}
