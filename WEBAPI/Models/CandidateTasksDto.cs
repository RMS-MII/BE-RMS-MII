using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class CandidateTasksDto
{
    public DateTime ServerTimeUtc { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public IReadOnlyCollection<CandidateApplicationTaskDto> Applications { get; set; } = [];
}

public sealed class CandidateApplicationTaskDto
{
    public Guid Id { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
    public Guid RecruitmentBatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public FinalResultStatus? FinalResultStatus { get; set; }
    public OfferingLetterStatus OfferingLetterStatus { get; set; }
    public string? FileOfferingLetter { get; set; }
    public DateTime AppliedAt { get; set; }
    public IReadOnlyCollection<CandidatePipelineDto> Pipeline { get; set; } = [];
    public IReadOnlyCollection<CandidateStageTaskDto> Stages { get; set; } = [];
}

public sealed class CandidateStageTaskDto
{
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string StageTypeName { get; set; } = string.Empty;
    public StageCategory? StageCategory { get; set; }
    public ApplicationStageStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime? AttemptStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsAttempts { get; set; }
    public TimeSpan? DurationTest { get; set; }
    public bool CanTakeTest { get; set; }
    public bool CanConfirmSchedule { get; set; }
    public bool CanUploadSubmission { get; set; }
    public bool CanRequestReschedule { get; set; }
    public IReadOnlyCollection<CandidateSessionTaskDto> Sessions { get; set; } = [];
}

public sealed class CandidateSessionTaskDto
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public string? FileTakeHome { get; set; }
    public string? FileSubmission { get; set; }
    public bool IsSubmitted { get; set; }
    public IReadOnlyCollection<string> Interviewers { get; set; } = [];
    public CandidateRescheduleDto? Reschedule { get; set; }
    public bool CanConfirm { get; set; }
    public bool CanUploadSubmission { get; set; }
    public bool CanRequestReschedule { get; set; }
}

public sealed record CandidatePipelineDto(Guid BatchStageId, string StageName, Guid? NextStageId);
public sealed record CandidateRescheduleDto(string Status, DateTime ProposedStart, DateTime ProposedEnd);
