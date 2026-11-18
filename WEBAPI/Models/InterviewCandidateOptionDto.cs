namespace WEBAPI.Models;

public sealed class InterviewCandidateOptionDto
{
    public Guid ApplicationId { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
}
