namespace WEBAPI.Models;

public sealed class SubmitAnswerResultDto
{
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SubmittedAnswerCount { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
