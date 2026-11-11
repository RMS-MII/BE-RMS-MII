using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class InterviewerDashboardSummaryDto
{
    public int PendingInvitationsCount { get; set; }
    public int UpcomingSessionsCount { get; set; }
    public int PendingAssessmentsCount { get; set; }
    public int CompletedAssessmentsCount { get; set; }
}

public class DashboardInterviewSessionDto
{
    public Guid SessionInterviewId { get; set; }
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Location { get; set; }
    public InterviewSessionStatus SessionStatus { get; set; }
    public string SessionStatusName => SessionStatus.ToDisplayName();
    public SessionInterviewStatus InterviewerStatus { get; set; }
    public string InterviewerStatusName => InterviewerStatus.ToDisplayName();
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public bool HasTakeHome { get; set; }
    public bool CanAssignTakeHome { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public string? FileTakeHome { get; set; }
    public string? FileSubmission { get; set; }
    public bool IsLead { get; set; }
    public bool IsAssessmentSubmitted { get; set; }
    public decimal? AssessmentScore { get; set; }
    public Recommendation? AssessmentRecommendation { get; set; }
    public string? AssessmentRecommendationName => AssessmentRecommendation?.ToDisplayName();
}

public class InterviewerDashboardDto
{
    public InterviewerDashboardSummaryDto Summary { get; set; } = new();
    public List<DashboardInterviewSessionDto> UpcomingSessions { get; set; } = [];
    public List<DashboardInterviewSessionDto> RecentSessions { get; set; } = [];
    public List<InterviewerInvitationDto> PendingInvitations { get; set; } = [];
}

