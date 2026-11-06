using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class QuestionBankMapping
{
    public static QuestionBankDetailDto ToDetailDto(this QuestionBank questionBank) => new()
    {
        Id = questionBank.Id,
        DepartmentId = questionBank.DepartmentId,
        DepartmentName = questionBank.Department?.Name ?? string.Empty,
        StageTypeId = questionBank.StageTypeId,
        StageTypeName = questionBank.StageType?.Name ?? string.Empty,
        CreatedBy = questionBank.CreatedBy,
        QuestionCount = questionBank.Questions.Count,
        ActiveQuestionCount = questionBank.Questions.Count(question => question.IsActive),
        CreatedAt = questionBank.CreatedAt,
        UpdatedAt = questionBank.UpdatedAt,
        Questions = questionBank.Questions
            .OrderBy(question => question.SequenceNumber)
            .ThenBy(question => question.Id)
            .Select(question => new QuestionBankQuestionDto
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Weight = question.Weight,
                SequenceNumber = question.SequenceNumber,
                IsRequired = question.IsRequired,
                IsActive = question.IsActive,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                Options = question.QuestionOptions
                    .OrderBy(option => option.SequenceNumber)
                    .ThenBy(option => option.Id)
                    .Select(option => new QuestionBankOptionDto
                    {
                        Id = option.Id,
                        OptionLabel = option.OptionLabel,
                        OptionText = option.OptionText,
                        Score = option.Score,
                        SequenceNumber = option.SequenceNumber,
                        IsActive = option.IsActive
                    })
                    .ToList()
            })
            .ToList()
    };
}
