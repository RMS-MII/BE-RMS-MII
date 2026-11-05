using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class SubmitAssessmentRequest
{
    public decimal Score { get; set; }
    public Recommendation Recommendation { get; set; }
    public string? Notes { get; set; }
}

public class AssessmentDto
{
    public Guid AssessmentId { get; set; }
    public Guid SessionInterviewId { get; set; }
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public Recommendation Recommendation { get; set; }
    public string RecommendationName => Recommendation.ToDisplayName();
    public string? Notes { get; set; }
    public SessionInterviewStatus InterviewerStatus { get; set; }
    public string InterviewerStatusName => InterviewerStatus.ToDisplayName();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

