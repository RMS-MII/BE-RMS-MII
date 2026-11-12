namespace WEBAPI.Models;

public sealed class InterviewerSessionDetailDto
{
    public DashboardInterviewSessionDto Session { get; set; } = new();
    public CandidateDto Candidate { get; set; } = new();
    public string? AssessmentNotes { get; set; }
    public bool CanAssess { get; set; }
}
