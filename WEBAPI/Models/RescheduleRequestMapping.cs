using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class RescheduleRequestMapping
{
    public static RescheduleRequestDto ToDto(this RescheduleRequest request, InterviewSession? session = null)
    {
        var candidate = session?.ApplicationStage?.BatchApplication?.Candidate;

        return new RescheduleRequestDto
        {
            Id = request.Id,
            InterviewSessionId = request.InterviewSessionId,
            SessionName = session?.SessionName ?? string.Empty,
            CandidateId = candidate?.Id ?? Guid.Empty,
            CandidateName = candidate?.FullName ?? string.Empty,
            OriginalStart = session?.ScheduledStart ?? DateTime.MinValue,
            OriginalEnd = session?.ScheduledEnd ?? DateTime.MinValue,
            RequesterEmail = request.RequesterEmail,
            ProposedStart = request.ProposedStart,
            ProposedEnd = request.ProposedEnd,
            Reason = request.Reason,
            Status = request.Status,
            StatusName = request.Status.ToDisplayName(),
            ReviewedBy = request.ReviewedBy,
            ReviewedAt = request.ReviewedAt,
            ReviewNotes = request.ReviewNotes,
            CreatedAt = request.CreatedAt
        };
    }
}

