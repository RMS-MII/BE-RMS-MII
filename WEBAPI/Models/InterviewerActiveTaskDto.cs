using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class InterviewerActiveTaskDto
{
    public InterviewerTaskType TaskType { get; set; }
    public string TaskTypeName => TaskType.ToDisplayName();
    public int Priority { get; set; }
    public bool IsActionable { get; set; }
    public bool IsOverdue { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? StageEmployeeId { get; set; }
    public Guid? BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid? StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public Guid? ApplicationStageId { get; set; }

    public Guid? InterviewSessionId { get; set; }
    public Guid? SessionInterviewId { get; set; }
    public string? SessionName { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }

    public Guid? CandidateId { get; set; }
    public string? CandidateName { get; set; }

    public DateTime? DueAt { get; set; }
    public bool IsLead { get; set; }
}
