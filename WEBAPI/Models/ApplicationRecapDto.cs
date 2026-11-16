using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class ApplicationRecapDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public bool IsStandalone { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public FinalResultStatus? FinalResultStatus { get; set; }
    public ApplicationStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public OfferingLetterStatus OfferingLetterStatus { get; set; }
    public string FileOfferingLetter { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public CandidateDto Candidate { get; set; } = new();
    public RecruitmentBatchSummaryDto Batch { get; set; } = new();
    public IReadOnlyCollection<ApplicationStageRecapDto> Stages { get; set; } = [];
    public ApplicationRecapSummaryDto Summary { get; set; } = new();
}

public class RecruitmentBatchSummaryDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BatchStatus Status { get; set; }
}

public class ApplicationStageRecapDto
{
    public Guid? ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public BatchStageLifecycleStatus LifecycleStatus { get; set; }
    public int Sequence { get; set; }
    public string StageName { get; set; } = string.Empty;
    public Guid StageTypeId { get; set; }
    public string? StageTypeName { get; set; }
    public StageCategory? StageCategory { get; set; }
    public decimal? PassingScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? NextStageId { get; set; }

    public ApplicationStageStatus? Status { get; set; }
    public decimal? TotalScore { get; set; }
    public IReadOnlyCollection<DiscSummaryDto> DiscSummaries { get; set; } = [];
    public DateTime? StartedAt { get; set; }
    public DateTime? EndAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsAttempts { get; set; }
    public TimeSpan? DurationTest { get; set; }

    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsPassed { get; set; }

    public IReadOnlyCollection<StageInterviewerDto> Interviewers { get; set; } = [];
    public IReadOnlyCollection<InterviewSessionRecapDto> InterviewSessions { get; set; } = [];
    public IReadOnlyCollection<CandidateAnswerDto> TestAnswers { get; set; } = [];
}

public class StageInterviewerDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public StageEmployeeStatus Status { get; set; }
    public string? Note { get; set; }
}

public class InterviewSessionRecapDto
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public string? FileSubmission { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public IReadOnlyCollection<SessionInterviewerRecapDto> Interviewers { get; set; } = [];
}

public class SessionInterviewerRecapDto
{
    public Guid Id { get; set; }
    public Guid StageEmployeeId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public SessionInterviewStatus Status { get; set; }
    public bool IsLead { get; set; }
    public InterviewAssessmentDto? Assessment { get; set; }
}

public class InterviewAssessmentDto
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
    public Recommendation Recommendation { get; set; }
    public decimal Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CandidateAnswerDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionOptionText { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ApplicationRecapSummaryDto
{
    public int TotalStages { get; set; }
    public int StartedStages { get; set; }
    public int CompletedStages { get; set; }
    public int PassedStages { get; set; }
    public int RejectedStages { get; set; }
    public Guid? CurrentBatchStageId { get; set; }
    public Guid? CurrentApplicationStageId { get; set; }
    public string? CurrentStageName { get; set; }
    public ApplicationStageStatus? CurrentStageStatus { get; set; }
    public bool IsAllStagesCompleted { get; set; }
    public bool CanFinalize { get; set; }
}
