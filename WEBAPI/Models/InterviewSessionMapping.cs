using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class InterviewSessionMapping
{
    public static InterviewSessionDto ToInterviewSessionDto(
        this InterviewSession session,
        bool hasPendingReschedule = false) => new()
    {
        Id = session.Id,
        ApplicationStageId = session.ApplicationStageId,
        BatchStageId = session.ApplicationStage?.BatchStageId ?? Guid.Empty,
        RecruitmentBatchId = session.ApplicationStage?.BatchApplication?.RecruitmentBatchId ?? Guid.Empty,
        CandidateId = session.ApplicationStage?.BatchApplication?.CandidateId ?? Guid.Empty,
        CodeApplication = session.ApplicationStage?.BatchApplication?.CodeApplication ?? string.Empty,
        CandidateName = session.ApplicationStage?.BatchApplication?.Candidate?.FullName ?? string.Empty,
        CandidateEmail = session.ApplicationStage?.BatchApplication?.Candidate?.Email ?? string.Empty,
        BatchName = session.ApplicationStage?.BatchStage?.RecruitmentBatch?.Name ?? string.Empty,
        StageName = session.ApplicationStage?.BatchStage?.StageName ?? string.Empty,
        SessionName = session.SessionName,
        ScheduledStart = session.ScheduledStart,
        ScheduledEnd = session.ScheduledEnd,
        IsOnline = session.IsOnline,
        Location = session.Location,
        MeetingUrl = session.MeetingUrl,
        Status = session.Status,
        Interviewers = session.SessionInterviews.ToScheduledInterviewerDtoList(),
        HasPendingReschedule = hasPendingReschedule,
        CreatedAt = session.CreatedAt
    };

    public static AssignTakeHomeResponse ToAssignTakeHomeResponse(
        this InterviewSession session,
        bool emailSent,
        string message)
    {
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;

        return new AssignTakeHomeResponse
        {
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            CandidateName = candidate?.FullName ?? string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            EndedSubmissionAt = session.EndedSubmissionAt ?? DateTime.MinValue,
            EmailSent = emailSent,
            Message = message
        };
    }

    public static SubmissionDto ToSubmissionDto(this InterviewSession session)
    {
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;
        var stage = session.ApplicationStage?.BatchStage;
        var batch = stage?.RecruitmentBatch;
        var hasTakeHome = session.EndedSubmissionAt.HasValue;
        var isSubmitted = !string.IsNullOrWhiteSpace(session.FileSubmission);
        var isOverdue = hasTakeHome && !isSubmitted && session.EndedSubmissionAt < DateTime.UtcNow;

        var status = isSubmitted
            ? "Telah Dikumpulkan"
            : (hasTakeHome ? (isOverdue ? "Terlambat / Melewati Tenggat" : "Menunggu Pengumpulan") : "Tidak Ada Tugas Take-Home");

        return new SubmissionDto
        {
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            CandidateId = candidate?.Id ?? Guid.Empty,
            CandidateName = candidate?.FullName ?? string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            BatchName = batch?.Name ?? string.Empty,
            StageName = stage?.StageName ?? string.Empty,
            HasTakeHome = hasTakeHome,
            EndedSubmissionAt = session.EndedSubmissionAt,
            IsSubmitted = isSubmitted,
            FileSubmission = session.FileSubmission,
            IsOverdue = isOverdue,
            Status = status
        };
    }
}
