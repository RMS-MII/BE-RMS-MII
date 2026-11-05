using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class ConfirmScheduleResultDto
{
    public Guid SessionId { get; set; }
    public Guid ApplicationStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public bool AlreadyConfirmed { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
