using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Commands.ConfirmSchedule;

public sealed class ConfirmScheduleHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ConfirmScheduleCommand, Result<ConfirmScheduleResultDto>>
{
    public async Task<Result<ConfirmScheduleResultDto>> Handle(
        ConfirmScheduleCommand request, CancellationToken ct)
    {
        var session = await db.InterviewSessions
            .Include(interviewSession => interviewSession.ApplicationStage)
                .ThenInclude(applicationStage => applicationStage.BatchApplication)
                    .ThenInclude(application => application.RecruitmentBatch)
            .Include(interviewSession => interviewSession.ApplicationStage)
                .ThenInclude(applicationStage => applicationStage.BatchStage)
            .FirstOrDefaultAsync(interviewSession =>
                interviewSession.Id == request.SessionId &&
                interviewSession.ApplicationStage.BatchApplication.CandidateId ==
                request.CandidateId, ct);

        if (session is null)
            return Result<ConfirmScheduleResultDto>.Failure(
                Error.NotFound("Interview session was not found."));

        var application = session.ApplicationStage.BatchApplication;

        if (application.Status.IsFinal() || application.FinalResultStatus is not null)
            return Result<ConfirmScheduleResultDto>.Failure(
                Error.BusinessRule("Application has already been finalized."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        if (session.Status == InterviewSessionStatus.AcceptedByCandidate)
            return Result<ConfirmScheduleResultDto>.Success(
                Map(session, alreadyConfirmed: true, now));

        if (session.Status != InterviewSessionStatus.AcceptedByInterviewer)
        {
            var message = session.Status switch
            {
                InterviewSessionStatus.PendingConfirmation or InterviewSessionStatus.Scheduled =>
                    "The interview session is still waiting for interviewer confirmation.",
                InterviewSessionStatus.Rescheduled =>
                    "The interview session has been rescheduled.",
                _ => "The interview session can no longer be confirmed."
            };

            return Result<ConfirmScheduleResultDto>.Failure(Error.BusinessRule(message));
        }

        var pending = await db.RescheduleRequests.AnyAsync(r => r.InterviewSessionId == session.Id && r.Status == RescheduleStatus.Pending, ct);
        if (pending || !CandidateActionPolicy.CanConfirm(session, now))
            return Result<ConfirmScheduleResultDto>.Failure(Error.BusinessRule("This schedule can no longer be confirmed."));

        session.Status = InterviewSessionStatus.AcceptedByCandidate;
        session.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return Result<ConfirmScheduleResultDto>.Success(
            Map(session, alreadyConfirmed: false, now));
    }

    private static ConfirmScheduleResultDto Map(
        InterviewSession session,
        bool alreadyConfirmed,
        DateTime now) => new()
    {
        SessionId = session.Id,
        ApplicationStageId = session.ApplicationStageId,
        StageName = session.ApplicationStage.BatchStage.StageName,
        SessionName = session.SessionName,
        ScheduledStart = session.ScheduledStart,
        ScheduledEnd = session.ScheduledEnd,
        IsOnline = session.IsOnline,
        Location = session.Location,
        MeetingUrl = session.MeetingUrl,
        Status = session.Status,
        AlreadyConfirmed = alreadyConfirmed,
        ServerTimeUtc = now
    };
}
