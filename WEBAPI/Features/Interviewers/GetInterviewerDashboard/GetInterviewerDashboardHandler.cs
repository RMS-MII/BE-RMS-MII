using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerDashboard;

public class GetInterviewerDashboardHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetInterviewerDashboardQuery, Result<InterviewerDashboardDto>>
{
    public async Task<Result<InterviewerDashboardDto>> Handle(
        GetInterviewerDashboardQuery query,
        CancellationToken ct)
    {
        var currentEmployeeId = currentUser.EmployeeId;
        if (currentEmployeeId is null || currentEmployeeId == Guid.Empty)
        {
            return Result<InterviewerDashboardDto>.Failure(
                Error.Forbidden("Akun Anda tidak terhubung dengan profil pegawai pewawancara."));
        }

        var now = DateTime.UtcNow;

        // 1. Ambil undangan penugasan tahapan yang masih pending untuk pewawancara ini (AsNoTracking untuk performa read-only)
        var pendingInvitations = await db.StageEmployees
            .AsNoTracking()
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.StageType)
            .Where(se => se.EmployeeId == currentEmployeeId.Value
                      && se.Status == StageEmployeeStatus.Pending
                      && se.IsActive
                      && se.BatchStage.RecruitmentBatch.Status != BatchStatus.Closed)
            .OrderByDescending(se => se.CreatedAt)
            .ToListAsync(ct);

        // 2. Ambil seluruh penugasan sesi wawancara untuk pewawancara ini (AsNoTracking)
        var mySessionInterviews = await db.SessionInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchStage)
                        .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchApplication)
                        .ThenInclude(ba => ba.Candidate)
            .Include(si => si.InterviewAssessment)
            .Include(si => si.StageEmployee)
            .Where(si => si.StageEmployee.EmployeeId == currentEmployeeId.Value)
            .ToListAsync(ct);

        var sessionIds = mySessionInterviews.Select(si => si.InterviewSessionId).Distinct().ToArray();
        var panelInterviewers = await db.SessionInterviews
            .AsNoTracking()
            .Include(panel => panel.StageEmployee)
                .ThenInclude(stageEmployee => stageEmployee.Employee)
            .Where(panel => sessionIds.Contains(panel.InterviewSessionId))
            .ToListAsync(ct);
        var panelsBySession = panelInterviewers
            .GroupBy(panel => panel.InterviewSessionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var session in mySessionInterviews)
        {
            session.InterviewSession.SessionInterviews = panelsBySession.GetValueOrDefault(session.InterviewSessionId) ?? [];
        }

        // 3. Filter sesi mendatang (upcoming)
        var upcomingSessions = mySessionInterviews
            .Where(si => si.InterviewSession.ScheduledEnd >= now
                      && si.Status != SessionInterviewStatus.Cancelled
                      && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled)
            .OrderBy(si => si.InterviewSession.ScheduledStart)
            .ToList();

        // 4. Filter sesi lampau / baru selesai (recent)
        var recentSessions = mySessionInterviews
            .Where(si => si.InterviewSession.ScheduledEnd < now
                      && si.Status != SessionInterviewStatus.Cancelled
                      && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled)
            .OrderByDescending(si => si.InterviewSession.ScheduledEnd)
            .Take(10)
            .ToList();

        // 5. Susun daftar task aktif yang bisa langsung dikerjakan pewawancara
        var activeTasks = BuildActiveTasks(pendingInvitations, mySessionInterviews, now);

        // 6. Hitung metrik penilaian
        var takeHomeReviewsCount = activeTasks.Count(task => task.TaskType == InterviewerTaskType.TakeHomeReview);
        var pendingAssessmentsCount = activeTasks.Count(task => task.TaskType == InterviewerTaskType.PendingAssessment);
        var sessionsToConfirmCount = activeTasks.Count(task => task.TaskType == InterviewerTaskType.SessionConfirmation);

        var completedAssessmentsCount = mySessionInterviews.Count(si => si.InterviewAssessment is not null);

        var dashboard = new InterviewerDashboardDto
        {
            ServerTimeUtc = now,
            Summary = new InterviewerDashboardSummaryDto
            {
                PendingInvitationsCount = pendingInvitations.Count,
                SessionsToConfirmCount = sessionsToConfirmCount,
                UpcomingSessionsCount = upcomingSessions.Count,
                PendingAssessmentsCount = pendingAssessmentsCount,
                TakeHomeReviewsCount = takeHomeReviewsCount,
                CompletedAssessmentsCount = completedAssessmentsCount,
                ActiveTasksCount = activeTasks.Count,
                OverdueTasksCount = activeTasks.Count(task => task.IsOverdue)
            },
            ActiveTasks = activeTasks,
            UpcomingSessions = upcomingSessions.Select(si => si.ToDashboardInterviewSessionDto()).ToList(),
            RecentSessions = recentSessions.Select(si => si.ToDashboardInterviewSessionDto()).ToList(),
            PendingInvitations = pendingInvitations.ToInterviewerInvitationDtoList()
        };

        return Result<InterviewerDashboardDto>.Success(dashboard);
    }

    private static List<InterviewerActiveTaskDto> BuildActiveTasks(
        IReadOnlyCollection<StageEmployee> pendingInvitations,
        IReadOnlyCollection<SessionInterview> sessionInterviews,
        DateTime now)
    {
        var tasks = new List<InterviewerActiveTaskDto>();

        tasks.AddRange(pendingInvitations.Select(stageEmployee => stageEmployee.ToPendingInvitationTask(now)));

        foreach (var sessionInterview in sessionInterviews)
        {
            var session = sessionInterview.InterviewSession;

            if (sessionInterview.Status == SessionInterviewStatus.Cancelled
                || session.Status == InterviewSessionStatus.Rescheduled
                || sessionInterview.StageEmployee?.Status == StageEmployeeStatus.Declined
                || session.ApplicationStage?.BatchStage?.RecruitmentBatch?.Status == BatchStatus.Closed)
            {
                continue;
            }

            var hasEnded = session.ScheduledEnd < now;
            var hasSubmission = !string.IsNullOrWhiteSpace(session.SubmissionFile);
            var assessmentMissing = sessionInterview.InterviewAssessment is null;
            var acceptedByCandidate = session.Status == InterviewSessionStatus.AcceptedByCandidate;

            if (session.Status == InterviewSessionStatus.PendingConfirmation)
            {
                if (sessionInterview.IsLead)
                {
                    tasks.Add(sessionInterview.ToSessionConfirmationTask(now));
                }
                else if (!hasEnded)
                {
                    tasks.Add(sessionInterview.ToUpcomingSessionTask(now));
                }

                continue;
            }

            if (acceptedByCandidate && assessmentMissing && hasSubmission)
            {
                tasks.Add(sessionInterview.ToTakeHomeReviewTask(now));
                continue;
            }

            if (acceptedByCandidate && assessmentMissing && hasEnded)
            {
                tasks.Add(sessionInterview.ToPendingAssessmentTask(now));
                continue;
            }

            if (!hasEnded)
            {
                tasks.Add(sessionInterview.ToUpcomingSessionTask(now));
            }
        }

        var orderedTasks = tasks
            .OrderByDescending(task => task.IsOverdue)
            .ThenByDescending(task => task.IsActionable)
            .ThenBy(task => task.DueAt ?? DateTime.MaxValue)
            .ThenBy(task => task.ScheduledStart ?? DateTime.MaxValue)
            .ThenBy(task => task.TaskType)
            .ToList();

        // Batasi task informasi agar payload dashboard tetap ringkas.
        // Seluruh sesi mendatang tetap tersedia di UpcomingSessions.
        const int maxInformationalTasks = 5;
        var limitedTasks = new List<InterviewerActiveTaskDto>(orderedTasks.Count);
        var informationalCount = 0;
        foreach (var task in orderedTasks)
        {
            if (!task.IsActionable)
            {
                if (informationalCount >= maxInformationalTasks) continue;
                informationalCount++;
            }

            limitedTasks.Add(task);
        }

        return limitedTasks;
    }
}
