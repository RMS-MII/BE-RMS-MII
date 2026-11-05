using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class RescheduleRequestDto
{
    public Guid Id { get; set; }
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public DateTime OriginalStart { get; set; }
    public DateTime OriginalEnd { get; set; }
    public string RequesterEmail { get; set; } = string.Empty;
    public DateTime ProposedStart { get; set; }
    public DateTime ProposedEnd { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RescheduleStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record RequestRescheduleRequest(
    Guid InterviewSessionId,
    DateTime ProposedStart,
    DateTime ProposedEnd,
    string Reason);

public record CandidateRequestRescheduleRequest(
    DateTime ProposedStart,
    DateTime ProposedEnd,
    string Reason);

public record ReviewRescheduleRequest(
    RescheduleStatus Decision,
    string? ReviewNotes);

