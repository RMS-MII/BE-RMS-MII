using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class ScheduledInterviewerDto
{
    public Guid SessionInterviewId { get; set; }
    public Guid StageEmployeeId { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public SessionInterviewStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
}

public class ConfirmSessionResponse
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
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public bool CandidateInvitationEmailSent { get; set; }
    public string Message { get; set; } = string.Empty;
}

