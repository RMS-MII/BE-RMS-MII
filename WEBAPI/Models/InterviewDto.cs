using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public record AssignInterviewersRequest(List<Guid> InterviewerEmployeeIds);

public class AssignedInterviewerDto
{
    public Guid StageEmployeeId { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
}

public class AssignInterviewersResponse
{
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public List<AssignedInterviewerDto> Interviewers { get; set; } = [];
}

public class InterviewerInvitationDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string StageTypeName { get; set; } = string.Empty;
    public DateTime StageStartDate { get; set; }
    public DateTime StageEndDate { get; set; }
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public string? Note { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public record RespondInvitationRequest(
    StageEmployeeStatus Response,
    string? Note = null);

public class RespondInvitationResponse
{
    public Guid InvitationId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public string? Note { get; set; }
    public DateTime RespondedAt { get; set; }
}

public class ScheduleInterviewSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; } = true;
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public List<Guid> InterviewerEmployeeIds { get; set; } = [];
}

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

public class InterviewSessionDto
{
    public Guid Id { get; set; }
    public Guid ApplicationStageId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public List<ScheduledInterviewerDto> Interviewers { get; set; } = [];
    public DateTime CreatedAt { get; set; }
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
