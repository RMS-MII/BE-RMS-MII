using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Applications.Queries.GetTestResult;

public sealed class GetTestResultHandler(RmsDbContext db)
    : IRequestHandler<GetTestResultQuery, Result<TestAttemptResultDto>>
{
    public async Task<Result<TestAttemptResultDto>> Handle(
        GetTestResultQuery request, CancellationToken ct)
    {
        var stage = await db.ApplicationStages
            .AsNoTracking()
            .Include(applicationStage => applicationStage.BatchStage)
                .ThenInclude(batchStage => batchStage.StageType)
            .Include(applicationStage => applicationStage.BatchApplication)
                .ThenInclude(application => application.Candidate)
            .Include(applicationStage => applicationStage.CandidateAnswers)
            .FirstOrDefaultAsync(applicationStage =>
                applicationStage.Id == request.ApplicationStageId, ct);

        if (stage is null)
            return Result<TestAttemptResultDto>.Failure(
                Error.NotFound("Test result was not found."));

        if (stage.BatchStage.StageType?.StageCategory != StageCategory.Test)
            return Result<TestAttemptResultDto>.Failure(
                Error.BusinessRule("The stage is not a test."));

        var answers = stage.CandidateAnswers
            .OrderBy(answer => answer.CreatedAt)
            .ThenBy(answer => answer.Id)
            .Select(answer => new CandidateAnswerDto
            {
                Id = answer.Id,
                QuestionText = answer.QuestionText,
                QuestionOptionText = answer.QuestionOptionText,
                Score = answer.Score,
                CreatedAt = answer.CreatedAt,
                UpdatedAt = answer.UpdatedAt
            })
            .ToList();

        var isTimedOut = stage.Status == ApplicationStageStatus.Expired ||
                         (stage.CompletedAt.HasValue &&
                          stage.AttemptStartedAt.HasValue &&
                          stage.DurationTest.HasValue &&
                          stage.CompletedAt.Value > stage.AttemptStartedAt.Value.Add(stage.DurationTest.Value));

        return Result<TestAttemptResultDto>.Success(new TestAttemptResultDto
        {
            ApplicationStageId = stage.Id,
            BatchStageId = stage.BatchStageId,
            BatchApplicationId = stage.BatchApplicationId,
            CandidateId = stage.BatchApplication.CandidateId,
            CandidateName = stage.BatchApplication.Candidate.FullName,
            CandidateEmail = stage.BatchApplication.Candidate.Email,
            BatchStageName = stage.BatchStage.StageName,
            StageTypeId = stage.BatchStage.StageTypeId,
            StageTypeName = stage.BatchStage.StageType?.Name,
            StageCategory = stage.BatchStage.StageType?.StageCategory,
            PassingScore = stage.BatchStage.PassingScore,
            Status = stage.Status,
            TotalScore = stage.TotalScore,
            StartedAt = stage.StartedAt,
            EndAt = stage.EndAt,
            AttemptStartedAt = stage.AttemptStartedAt,
            CompletedAt = stage.CompletedAt,
            DurationTest = stage.DurationTest,
            IsAttempts = stage.IsAttempts,
            IsTimedOut = isTimedOut,
            DiscSummaries = DiscSummaryCalculator.Summarize(stage.CandidateAnswers),
            Answers = answers
        });
    }
}
