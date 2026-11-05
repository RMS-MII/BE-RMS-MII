using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class CandidateTestDto
{
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string StageTypeName { get; set; } = string.Empty;
    public StageCategory StageCategory { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime ServerTimeUtc { get; set; }
    public TimeSpan? DurationTest { get; set; }
    public bool IsAttempts { get; set; }
    public int TotalQuestions { get; set; }
    public IReadOnlyCollection<CandidateTestQuestionDto> Questions { get; set; } = [];
}

public sealed class CandidateTestQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsRequired { get; set; }
    public IReadOnlyCollection<CandidateTestOptionDto> Options { get; set; } = [];
}

public sealed class CandidateTestOptionDto
{
    public Guid Id { get; set; }
    public string OptionLabel { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
}
