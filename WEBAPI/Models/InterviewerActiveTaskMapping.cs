using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class InterviewerActiveTaskMapping
{
    private const int ActionPriority = 1;
    private const int InformationPriority = 2;

    public static InterviewerActiveTaskDto ToPendingInvitationTask(
        this StageEmployee stageEmployee,
        DateTime now)
    {
        var batchStage = stageEmployee.BatchStage;
        var batch = batchStage?.RecruitmentBatch;
        var dueAt = batchStage?.EndDate;

        return new InterviewerActiveTaskDto
        {
            TaskType = InterviewerTaskType.PendingInvitation,
            Priority = ActionPriority,
            IsActionable = true,
            IsOverdue = dueAt.HasValue && dueAt.Value < now,
            Title = "Konfirmasi undangan pewawancara",
            Description = BuildInvitationDescription(batchStage?.StageName, batch?.Name),
            StageEmployeeId = stageEmployee.Id,
            BatchId = batchStage?.RecruitmentBatchId,
            BatchName = batch?.Name ?? string.Empty,
            StageId = stageEmployee.BatchStageId,
            StageName = batchStage?.StageName ?? string.Empty,
            DueAt = dueAt
        };
    }

    public static InterviewerActiveTaskDto ToSessionConfirmationTask(
        this SessionInterview sessionInterview,
        DateTime now)
    {
        var session = sessionInterview.InterviewSession;
        return CreateSessionTask(
            sessionInterview,
            InterviewerTaskType.SessionConfirmation,
            ActionPriority,
            isActionable: true,
            $"Konfirmasi jadwal wawancara {GetCandidateName(sessionInterview)}",
            "Tentukan apakah Anda dapat menghadiri sesi ini agar kandidat dapat menerima undangan.",
            session.ScheduledStart,
            now);
    }

    public static InterviewerActiveTaskDto ToTakeHomeReviewTask(
        this SessionInterview sessionInterview,
        DateTime now)
    {
        var session = sessionInterview.InterviewSession;
        return CreateSessionTask(
            sessionInterview,
            InterviewerTaskType.TakeHomeReview,
            ActionPriority,
            isActionable: true,
            $"Tinjau take-home {GetCandidateName(sessionInterview)}",
            "Kandidat sudah mengumpulkan take-home test. Tinjau hasilnya sebelum mengisi penilaian.",
            session.EndedSubmissionAt ?? session.ScheduledEnd,
            now);
    }

    public static InterviewerActiveTaskDto ToPendingAssessmentTask(
        this SessionInterview sessionInterview,
        DateTime now)
    {
        var session = sessionInterview.InterviewSession;
        return CreateSessionTask(
            sessionInterview,
            InterviewerTaskType.PendingAssessment,
            ActionPriority,
            isActionable: true,
            $"Lengkapi penilaian wawancara {GetCandidateName(sessionInterview)}",
            "Sesi wawancara sudah selesai. Isi skor, rekomendasi, dan catatan evaluasi.",
            session.ApplicationStage?.BatchStage?.EndDate,
            now);
    }

    public static InterviewerActiveTaskDto ToUpcomingSessionTask(
        this SessionInterview sessionInterview,
        DateTime now)
    {
        var session = sessionInterview.InterviewSession;
        return CreateSessionTask(
            sessionInterview,
            InterviewerTaskType.UpcomingSession,
            InformationPriority,
            isActionable: false,
            $"Sesi wawancara {GetCandidateName(sessionInterview)}",
            string.IsNullOrWhiteSpace(session.SessionName) ? null : session.SessionName,
            dueAt: null,
            now);
    }

    private static InterviewerActiveTaskDto CreateSessionTask(
        SessionInterview sessionInterview,
        InterviewerTaskType taskType,
        int priority,
        bool isActionable,
        string title,
        string? description,
        DateTime? dueAt,
        DateTime now)
    {
        var session = sessionInterview.InterviewSession;
        var applicationStage = session.ApplicationStage;
        var stage = applicationStage?.BatchStage;
        var batch = stage?.RecruitmentBatch;
        var candidate = applicationStage?.BatchApplication?.Candidate;

        return new InterviewerActiveTaskDto
        {
            TaskType = taskType,
            Priority = priority,
            IsActionable = isActionable,
            IsOverdue = isActionable && dueAt.HasValue && dueAt.Value < now,
            Title = title,
            Description = description,
            StageEmployeeId = sessionInterview.StageEmployeeId,
            BatchId = stage?.RecruitmentBatchId,
            BatchName = batch?.Name ?? string.Empty,
            StageId = stage?.Id,
            StageName = stage?.StageName ?? string.Empty,
            ApplicationStageId = applicationStage?.Id,
            InterviewSessionId = session.Id,
            SessionInterviewId = sessionInterview.Id,
            SessionName = session.SessionName,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            CandidateId = candidate?.Id,
            CandidateName = candidate?.FullName,
            DueAt = dueAt,
            IsLead = sessionInterview.IsLead
        };
    }

    private static string GetCandidateName(SessionInterview sessionInterview)
    {
        var candidateName = sessionInterview.InterviewSession
            .ApplicationStage?.BatchApplication?.Candidate?.FullName;
        return string.IsNullOrWhiteSpace(candidateName) ? "kandidat" : candidateName.Trim();
    }

    private static string BuildInvitationDescription(string? stageName, string? batchName)
    {
        var stage = string.IsNullOrWhiteSpace(stageName) ? "tahap wawancara" : stageName;
        return string.IsNullOrWhiteSpace(batchName)
            ? $"Anda ditugaskan sebagai pewawancara pada {stage}."
            : $"Anda ditugaskan sebagai pewawancara pada {stage} di batch {batchName}.";
    }
}
