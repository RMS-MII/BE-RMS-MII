namespace WEBAPI.Models;

public sealed class StartTestResultDto
{
    public Guid ApplicationStageId { get; set; }
    public DateTime AttemptStartedAt { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
