using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class ApplicationStage : BaseEntity
{
    public ApplicationStageStatus Status { get; set; } = ApplicationStageStatus.InProgress;
    public decimal? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsAttempts { get; set; } = false;
    public TimeSpan? DurationTest { get; set; }
    public Guid BatchApplicationId { get; set; }
    public Guid BatchStageId { get; set; }

    public BatchApplication BatchApplication { get; set; } = null!;
    public ICollection<InterviewSession> InterviewSessions { get; set; } = [];
    public ICollection<CandidateAnswer> CandidateAnswers { get; set; } = [];
    public BatchStage BatchStage { get; set; } = null!;
}
