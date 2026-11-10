using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Commands.SubmitTest;

public sealed class SubmitTestHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<SubmitTestCommand, Result<SubmitTestResultDto>>
{
    public async Task<Result<SubmitTestResultDto>> Handle(
        SubmitTestCommand request, CancellationToken ct)
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
            return Result<SubmitTestResultDto>.Failure(
                Error.NotFound("Test stage was not found or has not been started yet."));

        var application = stage.BatchApplication;

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        if (stageType.StageCategory != StageCategory.Test)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("This stage is not an online test."));

        switch (stage.Status)
        {
            case ApplicationStageStatus.Submitted:
                return Result<SubmitTestResultDto>.Failure(
                    Error.BusinessRule("This test has already been submitted."));
            case ApplicationStageStatus.Expired:
                return Result<SubmitTestResultDto>.Failure(
                    Error.BusinessRule("This test has expired."));
            case ApplicationStageStatus.InProgress:
                break;
            default:
                return Result<SubmitTestResultDto>.Failure(
                    Error.BusinessRule("This test can no longer be submitted."));
        }

        if (stage.IsAttempts)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("This test has already been attempted."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (stage.AttemptStartedAt is null)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("This test has not been started yet."));

        if (now < stage.StartedAt)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("This test has not started yet."));

        if (TestAttemptPolicy.IsPastDeadline(stage, now))
        {
            stage.Status = ApplicationStageStatus.Expired;
            stage.TotalScore = null;
            stage.CompletedAt = now;
            stage.UpdatedAt = now;
            application.UpdatedAt = now;

            await db.SaveChangesAsync(ct);

            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule(
                    "The test duration has ended. The stage has been marked as expired."));
        }

        var questions = await db.Questions
            .Where(question =>
                question.IsActive &&
                question.QuestionBank.DepartmentId == application.RecruitmentBatch.DepartmentId &&
                question.QuestionBank.StageTypeId == stage.BatchStage.StageTypeId)
            .Include(question => question.QuestionOptions)
            .OrderBy(question => question.SequenceNumber)
            .ThenBy(question => question.Id)
            .ToListAsync(ct);

        if (questions.Count == 0)
            return Result<SubmitTestResultDto>.Failure(
                Error.BusinessRule("No active questions are configured for this test."));

        if (request.Answers is null || request.Answers.Count == 0)
            return Result<SubmitTestResultDto>.Failure(
                Error.Validation("At least one answer is required."));

        var questionsById = questions.ToDictionary(question => question.Id);
        var answersByQuestion = new Dictionary<Guid, SubmitTestAnswerItem>();

        foreach (var answer in request.Answers)
        {
            if (!questionsById.ContainsKey(answer.QuestionId))
                return Result<SubmitTestResultDto>.Failure(
                    Error.Validation("One or more answers do not belong to this test."));

            if (answer.SelectedOptionIds is null || answer.SelectedOptionIds.Count == 0)
                return Result<SubmitTestResultDto>.Failure(
                    Error.Validation(
                        "Each answer must contain at least one selected option."));

            if (!answersByQuestion.TryAdd(answer.QuestionId, answer))
                return Result<SubmitTestResultDto>.Failure(
                    Error.Validation("Each question may be answered only once."));
        }

        var missingRequiredQuestion = questions
            .FirstOrDefault(question =>
                question.IsRequired && !answersByQuestion.ContainsKey(question.Id));

        if (missingRequiredQuestion is not null)
            return Result<SubmitTestResultDto>.Failure(
                Error.Validation(
                    "All required questions must be answered before submitting the test."));

        var candidateAnswers = new List<CandidateAnswer>();
        decimal totalScore = 0;

        foreach (var answer in answersByQuestion.Values)
        {
            var question = questionsById[answer.QuestionId];

            if (question.QuestionType == QuestionType.SingleChoice &&
                answer.SelectedOptionIds.Count != 1)
                return Result<SubmitTestResultDto>.Failure(
                    Error.Validation(
                        "Single-choice questions must have exactly one selected option."));

            var optionsById = question.QuestionOptions
                .Where(option => option.IsActive)
                .ToDictionary(option => option.Id);

            foreach (var optionId in answer.SelectedOptionIds.Distinct())
            {
                if (!optionsById.TryGetValue(optionId, out var option))
                    return Result<SubmitTestResultDto>.Failure(
                        Error.Validation(
                            "One or more selected options are invalid for the answered question."));

                var answerScore = Math.Round(
                    option.Score * question.Weight,
                    2,
                    MidpointRounding.AwayFromZero);

                totalScore += answerScore;

                candidateAnswers.Add(new CandidateAnswer
                {
                    Id = Guid.CreateVersion7(),
                    ApplicationStageId = stage.Id,
                    QuestionText = question.QuestionText,
                    QuestionOptionText = option.OptionText,
                    Score = answerScore,
                    CreatedAt = now
                });
            }
        }

        totalScore = Math.Round(totalScore, 2, MidpointRounding.AwayFromZero);

        stage.Status = ApplicationStageStatus.Submitted;
        stage.TotalScore = totalScore;
        stage.CompletedAt = now;
        stage.UpdatedAt = now;
        stage.IsAttempts = true;
        application.UpdatedAt = now;

        db.CandidateAnswers.AddRange(candidateAnswers);
        await db.SaveChangesAsync(ct);

        return Result<SubmitTestResultDto>.Success(new SubmitTestResultDto
        {
            ApplicationStageId = stage.Id,
            BatchStageId = stage.BatchStageId,
            StageName = stage.BatchStage.StageName,
            Status = stage.Status,
            TotalScore = stage.TotalScore,
            CompletedAt = stage.CompletedAt,
            ServerTimeUtc = now
        });
    }
}
