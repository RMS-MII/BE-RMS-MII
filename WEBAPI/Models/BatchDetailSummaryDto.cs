namespace WEBAPI.Models;

public sealed class BatchDetailSummaryDto
{
    public string? OwnerName { get; set; }
    public int TotalCandidates { get; set; }
    public int ActiveCandidates { get; set; }
    public int RejectedCandidates { get; set; }
    public int DroppedCandidates { get; set; }
    public int InterviewSessions { get; set; }
}
