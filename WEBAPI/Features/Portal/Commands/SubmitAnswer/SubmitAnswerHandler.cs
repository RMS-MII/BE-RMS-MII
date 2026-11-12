using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Portal.Commands.SubmitTest;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Commands.SubmitAnswer;

public sealed class SubmitAnswerHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ITestAttemptFinalizer finalizer)
    : IRequestHandler<SubmitAnswerCommand, Result<SubmitAnswerResultDto>>
{
    public async Task<Result<SubmitAnswerResultDto>> Handle(
        SubmitAnswerCommand request, CancellationToken ct)
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
            return Result<SubmitAnswerResultDto>.Failure(
                Error.NotFound("Test stage was not found or has not been started yet."));

        if (stage.BatchStage.StageType?.StageCategory == StageCategory.Test &&
            (stage.IsAttempts || stage.Status == ApplicationStageStatus.Submitted))
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("This test has already been submitted."));

        var application = stage.BatchApplication;

        if (application.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("This recruitment batch has closed."));

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var stageType = stage.BatchStage.StageType;

        if (stageType is null)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("The stage does not have a stage type."));

        if (stageType.StageCategory != StageCategory.Test)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("This stage is not an online test."));

        switch (stage.Status)
        {
            case ApplicationStageStatus.Expired:
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.BusinessRule("This test has expired."));
            case ApplicationStageStatus.InProgress:
                break;
            default:
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.BusinessRule("This test can no longer be submitted."));
        }

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (stage.AttemptStartedAt is null)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("This test has not been started yet."));

        if (now < stage.StartedAt)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("This test has not started yet."));

        // Validasi ketat batas waktu dilakukan di server berbasis timestamp server.
        if (TestAttemptPolicy.IsPastDeadline(stage, now))
        {
            stage.Status = ApplicationStageStatus.Expired;
            stage.CompletedAt = now;
            stage.UpdatedAt = now;
            application.UpdatedAt = now;

            await db.SaveChangesAsync(ct);

            // Batas waktu habis: sesi ditutup dan jawaban yang berhasil tersimpan
            // tetap dipertahankan untuk dihitung skornya.
            await finalizer.FinalizeAsync(stage, now, ct);

            return Result<SubmitAnswerResultDto>.Failure(
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
            return Result<SubmitAnswerResultDto>.Failure(
                Error.BusinessRule("No active questions are configured for this test."));

        if (request.Answers is null || request.Answers.Count == 0)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.Validation("At least one answer is required."));

        var questionsById = questions.ToDictionary(question => question.Id);
        var answersByQuestion = new Dictionary<Guid, SubmitTestAnswerItem>();

        foreach (var answer in request.Answers)
        {
            if (!questionsById.ContainsKey(answer.QuestionId))
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.Validation("One or more answers do not belong to this test."));

            if (answer.SelectedOptionIds is null || answer.SelectedOptionIds.Count == 0)
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.Validation(
                        "Each answer must contain at least one selected option."));

            if (!answersByQuestion.TryAdd(answer.QuestionId, answer))
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.Validation("Each question may be answered only once."));
        }

        var missingRequiredQuestion = questions
            .FirstOrDefault(question =>
                question.IsRequired && !answersByQuestion.ContainsKey(question.Id));

        if (missingRequiredQuestion is not null)
            return Result<SubmitAnswerResultDto>.Failure(
                Error.Validation(
                    "All required questions must be answered before submitting the test."));

        var candidateAnswers = new List<CandidateAnswer>();
        decimal totalScore = 0;

        foreach (var answer in answersByQuestion.Values)
        {
            var question = questionsById[answer.QuestionId];

            if (question.QuestionType == QuestionType.SingleChoice &&
                answer.SelectedOptionIds.Count != 1)
                return Result<SubmitAnswerResultDto>.Failure(
                    Error.Validation(
                        "Single-choice questions must have exactly one selected option."));

            var optionsById = question.QuestionOptions
                .Where(option => option.IsActive)
                .ToDictionary(option => option.Id);

            foreach (var optionId in answer.SelectedOptionIds.Distinct())
            {
                if (!optionsById.TryGetValue(optionId, out var option))
                    return Result<SubmitAnswerResultDto>.Failure(
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
                    DiscType = question.DiscType,
                    QuestionText = question.QuestionText,
                    QuestionOptionText = option.OptionText,
                    Score = answerScore,
                    CreatedAt = now
                });
            }
        }

        var maxRawScore = TestScoreCalculator.CalculateMaxRawScore(questions);
        stage.TotalScore = TestScoreCalculator.Normalize(totalScore, maxRawScore);
        stage.UpdatedAt = now;

        db.CandidateAnswers.AddRange(candidateAnswers);
        await db.SaveChangesAsync(ct);

        return Result<SubmitAnswerResultDto>.Success(new SubmitAnswerResultDto
        {
            ApplicationStageId = stage.Id,
            BatchStageId = stage.BatchStageId,
            StageName = stage.BatchStage.StageName,
            SubmittedAnswerCount = answersByQuestion.Count,
            ServerTimeUtc = now
        });
    }
}
