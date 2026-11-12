using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

/// <summary>
/// Hasil attempt tes daring (misalnya tes kepribadian tipe Forced-Choice seperti DISC)
/// yang dapat dilihat oleh HR.
/// </summary>
public class TestAttemptResultDto
{
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public Guid BatchApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateEmail { get; set; }
    public string BatchStageName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string? StageTypeName { get; set; }
    public StageCategory? StageCategory { get; set; }
    public decimal? PassingScore { get; set; }

    public ApplicationStageStatus Status { get; set; }
    public decimal? TotalScore { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime? AttemptStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? DurationTest { get; set; }
    public bool IsAttempts { get; set; }
    public bool IsTimedOut { get; set; }

    public IReadOnlyCollection<DiscSummaryDto> DiscSummaries { get; set; } = [];

    public IReadOnlyCollection<CandidateAnswerDto> Answers { get; set; } = [];
}
