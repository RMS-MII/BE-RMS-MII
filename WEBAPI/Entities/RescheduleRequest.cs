using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class RescheduleRequest : BaseEntity
{
    public Guid InterviewSessionId { get; set; }
    public string RequesterEmail { get; set; } = string.Empty;
    public DateTime ProposedStart { get; set; }
    public DateTime ProposedEnd { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RescheduleStatus Status { get; set; } = RescheduleStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}
