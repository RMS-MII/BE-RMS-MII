namespace WEBAPI.Models;

public sealed class FinishTestResultDto
{
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
