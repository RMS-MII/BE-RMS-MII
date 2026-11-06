using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

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

public class InterviewSessionDto
{
    public Guid Id { get; set; }
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public Guid CandidateId { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
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
    public bool HasPendingReschedule { get; set; }
    public Guid? PendingRescheduleId { get; set; }
    public DateTime? ProposedStart { get; set; }
    public DateTime? ProposedEnd { get; set; }
    public string? RescheduleReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignTakeHomeRequest
{
    public DateTime EndedSubmissionAt { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public class AssignTakeHomeResponse
{
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public DateTime EndedSubmissionAt { get; set; }
    public bool EmailSent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SubmissionDto
{
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public bool HasTakeHome { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public bool IsSubmitted { get; set; }
    public string? FileSubmission { get; set; }
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
}
