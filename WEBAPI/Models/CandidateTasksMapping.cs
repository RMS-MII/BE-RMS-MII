using WEBAPI.Features.Batches;
using WEBAPI.Features.Portal;
using WEBAPI.Common;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class CandidateTasksMapping
{
    public static CandidateApplicationTaskDto ToCandidateApplicationTaskDto(
        this BatchApplication application, DateTime now, IReadOnlyCollection<RescheduleRequest>? requests = null)
    {
        var usesLifecycle = !application.IsStandalone && application.RecruitmentBatch?.BatchStages.Any(stage =>
            stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive) == true;
        var unpublishedDecision = usesLifecycle && application.ApplicationStages.Any(stage =>
            stage.BatchStage?.LifecycleStatus == BatchStageLifecycleStatus.Active &&
            stage.Status.IsTerminalStage());
        var orderedStages = BatchStageSequence.GetOrderedStages(application.RecruitmentBatch?.BatchStages.ToList() ?? []);
        var positions = orderedStages.Select((stage, index) => (stage.Id, index))
            .ToDictionary(item => item.Id, item => item.index);
        return new CandidateApplicationTaskDto
        {
            Id = application.Id,
            CodeApplication = application.CodeApplication,
            RecruitmentBatchId = application.RecruitmentBatchId,
            BatchName = application.RecruitmentBatch?.Name ?? string.Empty,
            DepartmentId = application.RecruitmentBatch?.DepartmentId ?? Guid.Empty,
            DepartmentName = application.RecruitmentBatch?.Department?.Name ?? string.Empty,
            Status = unpublishedDecision && application.FinalResultStatus is null
                ? ApplicationStatus.InProcess
                : application.Status,
            FinalResultStatus = application.FinalResultStatus,
            OfferingLetterStatus = application.OfferingLetterStatus,
            FileOfferingLetter = string.IsNullOrWhiteSpace(application.FileOfferingLetter)
                ? null
                : application.FileOfferingLetter,
            AppliedAt = application.AppliedAt,
            Pipeline = orderedStages
                .Select(s => new CandidatePipelineDto(s.Id, s.StageName, s.NextStageId)).ToList(),
            Stages = application.ApplicationStages
                .OrderBy(stage => positions.GetValueOrDefault(stage.BatchStageId, int.MaxValue))
                .ThenBy(stage => stage.CreatedAt)
                .Select(stage => stage.ToCandidateStageTaskDto(now, requests, usesLifecycle))
                .ToList()
        };
    }

    private static CandidateStageTaskDto ToCandidateStageTaskDto(
        this ApplicationStage stage, DateTime now, IReadOnlyCollection<RescheduleRequest>? requests,
        bool usesLifecycle)
    {
        var hideDecision = usesLifecycle &&
            stage.BatchStage?.LifecycleStatus != BatchStageLifecycleStatus.Completed &&
            stage.Status.IsTerminalStage();
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
            Status = hideDecision
                ? stage.BatchStage?.StageType?.StageCategory == StageCategory.Test && stage.IsAttempts
                    ? ApplicationStageStatus.Submitted
                    : ApplicationStageStatus.InProgress
                : stage.Status,
            StartedAt = stage.StartedAt,
            EndAt = stage.EndAt,
            AttemptStartedAt = stage.AttemptStartedAt,
            CompletedAt = hideDecision ? null : stage.CompletedAt,
            IsAttempts = stage.IsAttempts,
            DurationTest = stage.DurationTest,
            CanTakeTest = CandidateActionPolicy.IsActive(stage) && isTestStage &&
                          (canStartTest || canContinueTest),
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
        var latest = changes.Where(r => r.RequestedByCandidate)
            .OrderByDescending(r => r.CreatedAt).FirstOrDefault();
        var pending = changes.Any(r => r.Status == RescheduleStatus.Pending);
        var submissionLocked = session.SessionInterviews.Any(interviewer =>
            interviewer.Status == SessionInterviewStatus.Completed || interviewer.InterviewAssessment is not null);

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
            FileTakeHome = session.FileTakeHome,
            FileSubmission = session.SubmissionFile,
            IsSubmitted = !string.IsNullOrWhiteSpace(session.SubmissionFile),
            IsSubmissionLocked = submissionLocked,
            Interviewers = session.SessionInterviews.Where(s => s.Status != SessionInterviewStatus.Cancelled)
                .Select(s => s.StageEmployee.Employee.FullName).Distinct().ToList(),
            Reschedule = latest is null ? null : new CandidateRescheduleDto(latest.Status.ToString(), latest.ProposedStart, latest.ProposedEnd),
            CanConfirm = CandidateActionPolicy.CanConfirm(session, now) && !pending,
            CanUploadSubmission = !submissionLocked && CandidateActionPolicy.CanSubmit(session, now),
            CanRequestReschedule = CandidateActionPolicy.CanReschedule(session, now, pending)
        };
    }
}
