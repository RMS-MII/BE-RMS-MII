using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
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
                      && se.IsActive)
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

        // 5. Hitung metrik penilaian
        var pendingAssessmentsCount = mySessionInterviews.Count(si =>
            si.InterviewAssessment is null
            && si.Status != SessionInterviewStatus.Cancelled
            && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled);

        var completedAssessmentsCount = mySessionInterviews.Count(si => si.InterviewAssessment is not null);

        var dashboard = new InterviewerDashboardDto
        {
            Summary = new InterviewerDashboardSummaryDto
            {
                PendingInvitationsCount = pendingInvitations.Count,
                UpcomingSessionsCount = upcomingSessions.Count,
                PendingAssessmentsCount = pendingAssessmentsCount,
                CompletedAssessmentsCount = completedAssessmentsCount
            },
            UpcomingSessions = upcomingSessions.Select(si => si.ToDashboardInterviewSessionDto()).ToList(),
            RecentSessions = recentSessions.Select(si => si.ToDashboardInterviewSessionDto()).ToList(),
            PendingInvitations = pendingInvitations.ToInterviewerInvitationDtoList()
        };

        return Result<InterviewerDashboardDto>.Success(dashboard);
    }
}
