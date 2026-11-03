using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class ApplicationStage : BaseEntity
{
    public ApplicationStatus Status { get; set; } = ApplicationStatus.InProcess;
    public decimal? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsAttempt { get; set; } =  false;
    public int DurationTest { get; set; }
    public Guid BatchApplicationId { get; set; }
    public Guid BatchStageId { get; set; }

    public BatchApplication BatchApplication { get; set; } = null!;

    //TODO : BatchStage added foreign Key
}