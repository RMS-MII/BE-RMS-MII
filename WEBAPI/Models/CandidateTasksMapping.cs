using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class CandidateTasksMapping
{
    public static CandidateApplicationTaskDto ToCandidateApplicationTaskDto(
        this BatchApplication application, DateTime now) => new()
    {
        Id = application.Id,
        CodeApplication = application.CodeApplication,
        RecruitmentBatchId = application.RecruitmentBatchId,
        BatchName = application.RecruitmentBatch?.Name ?? string.Empty,
        DepartmentId = application.RecruitmentBatch?.DepartmentId ?? Guid.Empty,
        DepartmentName = application.RecruitmentBatch?.Department?.Name ?? string.Empty,
        Status = application.Status,
        FinalResultStatus = application.FinalResultStatus,
        OfferingLetterStatus = application.OfferingLetterStatus,
        FileOfferingLetter = string.IsNullOrWhiteSpace(application.FileOfferingLetter)
            ? null
            : application.FileOfferingLetter,
        AppliedAt = application.AppliedAt,
        Stages = application.ApplicationStages
            .OrderBy(stage => stage.BatchStage?.StartDate ?? stage.StartedAt)
            .ThenBy(stage => stage.CreatedAt)
            .Select(stage => stage.ToCandidateStageTaskDto(now))
            .ToList()
    };

    private static CandidateStageTaskDto ToCandidateStageTaskDto(
        this ApplicationStage stage, DateTime now)
    {
        var sessions = stage.InterviewSessions
            .OrderBy(session => session.ScheduledStart)
            .Select(session => session.ToCandidateSessionTaskDto(now))
            .ToList();

        var stageType = stage.BatchStage?.StageType;

        return new CandidateStageTaskDto
        {
            ApplicationStageId = stage.Id,
            BatchStageId = stage.BatchStageId,
            StageName = stage.BatchStage?.StageName ?? string.Empty,
            StageTypeId = stage.BatchStage?.StageTypeId ?? Guid.Empty,
            StageTypeName = stageType?.Name ?? string.Empty,
            StageCategory = stageType?.StageCategory,
            Status = stage.Status,
            StartedAt = stage.StartedAt,
            EndAt = stage.EndAt,
            CompletedAt = stage.CompletedAt,
            IsAttempts = stage.IsAttempts,
            DurationTest = stage.DurationTest,
            CanTakeTest = stageType?.StageCategory == StageCategory.Test
                          && stage.Status == ApplicationStageStatus.InProgress
                          && !stage.IsAttempts
                          && now <= stage.EndAt,
            CanConfirmSchedule = sessions.Any(session => session.CanConfirm),
            CanUploadSubmission = sessions.Any(session => session.CanUploadSubmission),
            CanRequestReschedule = sessions.Any(session => session.CanRequestReschedule),
            Sessions = sessions
        };
    }

    private static CandidateSessionTaskDto ToCandidateSessionTaskDto(
        this InterviewSession session, DateTime now)
    {
        var isActive = session.Status != InterviewSessionStatus.Rescheduled;
        var isBeforeStart = session.ScheduledStart > now;
        var withinSubmissionDeadline = session.EndedSubmissionAt.HasValue
                                       && now <= session.EndedSubmissionAt.Value;

        return new CandidateSessionTaskDto
        {
            Id = session.Id,
            SessionName = session.SessionName,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            IsOnline = session.IsOnline,
            Location = session.Location,
            MeetingUrl = session.MeetingUrl,
            Status = session.Status,
            EndedSubmissionAt = session.EndedSubmissionAt,
            FileSubmission = session.FileSubmission,
            IsSubmitted = !string.IsNullOrWhiteSpace(session.FileSubmission),
            CanConfirm = isActive
                         && session.Status == InterviewSessionStatus.AcceptedByInterviewer,
            CanUploadSubmission = isActive && withinSubmissionDeadline,
            CanRequestReschedule = isActive && isBeforeStart
        };
    }
}
