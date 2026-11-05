using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class CandidateTestMapping
{
    public static CandidateTestDto ToCandidateTestDto(
        this ApplicationStage stage,
        IReadOnlyCollection<Question> questions,
        DateTime now)
    {
        var application = stage.BatchApplication;
        var batchStage = stage.BatchStage;
        var stageType = batchStage.StageType;

        return new CandidateTestDto
        {
            CandidateId = application.CandidateId,
            CandidateName = application.Candidate.FullName,
            ApplicationId = application.Id,
            ApplicationStageId = stage.Id,
            BatchStageId = batchStage.Id,
            StageName = batchStage.StageName,
            StageTypeId = batchStage.StageTypeId,
            StageTypeName = stageType.Name,
            StageCategory = stageType.StageCategory,
            StartedAt = stage.StartedAt,
            EndAt = stage.EndAt,
            ServerTimeUtc = now,
            DurationTest = stage.DurationTest,
            IsAttempts = stage.IsAttempts,
            TotalQuestions = questions.Count,
            Questions = questions
                .OrderBy(question => question.SequenceNumber)
                .ThenBy(question => question.Id)
                .Select(question => new CandidateTestQuestionDto
                {
                    Id = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    SequenceNumber = question.SequenceNumber,
                    IsRequired = question.IsRequired,
                    Options = question.QuestionOptions
                        .Where(option => option.IsActive)
                        .OrderBy(option => option.SequenceNumber)
                        .ThenBy(option => option.Id)
                        .Select(option => new CandidateTestOptionDto
                        {
                            Id = option.Id,
                            OptionLabel = option.OptionLabel,
                            OptionText = option.OptionText,
                            SequenceNumber = option.SequenceNumber
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
