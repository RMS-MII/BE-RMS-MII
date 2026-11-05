using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class SubmitTestResultDto
{
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public ApplicationStageStatus Status { get; set; }
    public decimal? TotalScore { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
