using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class QuestionBankDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string StageTypeName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public int QuestionCount { get; set; }
    public int ActiveQuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class QuestionBankDetailDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string StageTypeName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public int QuestionCount { get; set; }
    public int ActiveQuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<QuestionBankQuestionDto> Questions { get; set; } = [];
}

public sealed class QuestionBankQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public decimal Weight { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<QuestionBankOptionDto> Options { get; set; } = [];
}

public sealed class QuestionBankOptionDto
{
    public Guid Id { get; set; }
    public string OptionLabel { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsActive { get; set; }
}
