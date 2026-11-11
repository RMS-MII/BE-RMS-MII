using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.Portal;

public static class CandidateActionPolicy
{
    public static bool IsActive(ApplicationStage stage) =>
        (stage.BatchApplication.RecruitmentBatch ?? stage.BatchStage?.RecruitmentBatch)?.Status != BatchStatus.Closed
        && !stage.BatchApplication.Status.IsFinal() && stage.BatchApplication.FinalResultStatus is null
        && !stage.Status.IsTerminalStage();

    public static bool CanConfirm(InterviewSession session, DateTime now) =>
        IsActive(session.ApplicationStage) && session.ScheduledStart > now
        && session.Status == InterviewSessionStatus.AcceptedByInterviewer;

    public static bool CanReschedule(InterviewSession session, DateTime now, bool pending = false) =>
        IsActive(session.ApplicationStage) && !pending && session.ScheduledStart > now
        && session.Status is InterviewSessionStatus.AcceptedByInterviewer or InterviewSessionStatus.AcceptedByCandidate;

    public static bool CanSubmit(InterviewSession session, DateTime now) =>
        IsActive(session.ApplicationStage) && session.Status != InterviewSessionStatus.Rescheduled
        && session.EndedSubmissionAt.HasValue && now <= session.EndedSubmissionAt.Value;
}
