using WEBAPI.Features.Portal;
using WEBAPI.Common;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class CandidateTasksMapping
{
    public static CandidateApplicationTaskDto ToCandidateApplicationTaskDto(
        this BatchApplication application, DateTime now, IReadOnlyCollection<RescheduleRequest>? requests = null) => new()
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
        Pipeline = (application.RecruitmentBatch?.BatchStages ?? []).Where(s => s.IsActive)
            .OrderBy(s => s.StartDate).ThenBy(s => s.CreatedAt)
            .Select(s => new CandidatePipelineDto(s.Id, s.StageName, s.NextStageId)).ToList(),
        Stages = application.ApplicationStages
            .OrderBy(stage => stage.BatchStage?.StartDate ?? stage.StartedAt)
            .ThenBy(stage => stage.CreatedAt)
            .Select(stage => stage.ToCandidateStageTaskDto(now, requests))
            .ToList()
    };

    private static CandidateStageTaskDto ToCandidateStageTaskDto(
        this ApplicationStage stage, DateTime now, IReadOnlyCollection<RescheduleRequest>? requests)
    {
        var sessions = stage.InterviewSessions
            .OrderBy(session => session.ScheduledStart)
            .Select(session => session.ToCandidateSessionTaskDto(now, requests))
            .ToList();

        var stageType = stage.BatchStage?.StageType;

        var isTestStage = stageType?.StageCategory == StageCategory.Test
                          && stage.Status == ApplicationStageStatus.InProgress
                          && !stage.IsAttempts;

        var canStartTest = stage.AttemptStartedAt is null
                           && now >= stage.StartedAt
                           && now <= stage.EndAt;

        var canContinueTest = stage.AttemptStartedAt is not null
                              && !TestAttemptPolicy.IsPastDeadline(stage, now);

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
            AttemptStartedAt = stage.AttemptStartedAt,
            CompletedAt = stage.CompletedAt,
            IsAttempts = stage.IsAttempts,
            DurationTest = stage.DurationTest,
            CanTakeTest = CandidateActionPolicy.IsActive(stage) && stageType?.StageCategory == StageCategory.Test
                          && stage.Status == ApplicationStageStatus.InProgress
                          && !stage.IsAttempts
                          && now >= stage.StartedAt
                          && now <= stage.EndAt,
            CanConfirmSchedule = sessions.Any(session => session.CanConfirm),
            CanUploadSubmission = sessions.Any(session => session.CanUploadSubmission),
            CanRequestReschedule = sessions.Any(session => session.CanRequestReschedule),
            Sessions = sessions
        };
    }

    private static CandidateSessionTaskDto ToCandidateSessionTaskDto(
        this InterviewSession session, DateTime now, IReadOnlyCollection<RescheduleRequest>? requests)
    {
        var changes = requests?.Where(r => r.InterviewSessionId == session.Id).ToList() ?? [];
        var latest = changes.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
        var pending = changes.Any(r => r.Status == RescheduleStatus.Pending);

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
            Interviewers = session.SessionInterviews.Where(s => s.Status != SessionInterviewStatus.Cancelled)
                .Select(s => s.StageEmployee.Employee.FullName).Distinct().ToList(),
            Reschedule = latest is null ? null : new CandidateRescheduleDto(latest.Status.ToString(), latest.ProposedStart, latest.ProposedEnd),
            CanConfirm = CandidateActionPolicy.CanConfirm(session, now) && !pending,
            CanUploadSubmission = CandidateActionPolicy.CanSubmit(session, now),
            CanRequestReschedule = CandidateActionPolicy.CanReschedule(session, now, pending)
        };
    }
}
