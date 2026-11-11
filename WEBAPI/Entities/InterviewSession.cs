using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class InterviewSession : BaseEntity
{
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public string? FileTakeHome { get; set; }
    public string? SubmissionFile { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public Guid ApplicationStageId { get; set; }

    // Navigation properties
    public ApplicationStage ApplicationStage { get; set; } = null!;
    public ICollection<SessionInterview> SessionInterviews { get; set; } = [];
}
