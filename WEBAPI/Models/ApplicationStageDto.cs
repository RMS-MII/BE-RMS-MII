using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class ApplicationStageDto
{
    public Guid Id { get; set; }
    public Guid BatchApplicationId { get; set; }
    public Guid BatchStageId { get; set; }
    public string BatchStageName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string? StageTypeName { get; set; }
    public StageCategory? StageCategory { get; set; }
    public decimal? PassingScore { get; set; }

    public ApplicationStageStatus Status { get; set; }
    public decimal? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsAttempts { get; set; }
    public TimeSpan? DurationTest { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
