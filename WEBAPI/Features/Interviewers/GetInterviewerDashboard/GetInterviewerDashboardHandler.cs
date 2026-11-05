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

        var empId = currentEmployeeId.Value;
        var now = DateTime.UtcNow;

        // 1. Ambil undangan penugasan tahapan yang masih pending untuk pewawancara ini
        var pendingInvitations = await db.StageEmployees
            .AsNoTracking()
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.StageType)
            .Where(se => se.EmployeeId == empId
                      && se.Status == StageEmployeeStatus.Pending
                      && se.IsActive)
            .OrderByDescending(se => se.CreatedAt)
            .ToListAsync(ct);

        // 2. Base query untuk SessionInterviews milik pewawancara ini (tidak termasuk yang cancelled/rescheduled)
        var baseQuery = db.SessionInterviews
            .AsNoTracking()
            .AsSplitQuery()
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
            .Where(si => si.StageEmployee.EmployeeId == empId
                      && si.Status != SessionInterviewStatus.Cancelled
                      && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled);

        // 3. Sesi mendatang (upcoming) — filter di DB level
        var upcomingSessions = await baseQuery
            .Where(si => si.InterviewSession.ScheduledEnd >= now)
            .OrderBy(si => si.InterviewSession.ScheduledStart)
            .ToListAsync(ct);

        // 4. Sesi lampau / baru selesai (recent) — filter di DB level, ambil 10 terakhir
        var recentSessions = await baseQuery
            .Where(si => si.InterviewSession.ScheduledEnd < now)
            .OrderByDescending(si => si.InterviewSession.ScheduledEnd)
            .Take(10)
            .ToListAsync(ct);

        // 5. Hitung metrik penilaian — count di DB level (efisien tanpa load semua data)
        var pendingAssessmentsCount = await db.SessionInterviews
            .AsNoTracking()
            .CountAsync(si => si.StageEmployee.EmployeeId == empId
                           && si.InterviewAssessment == null
                           && si.Status != SessionInterviewStatus.Cancelled
                           && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled, ct);

        var completedAssessmentsCount = await db.SessionInterviews
            .AsNoTracking()
            .CountAsync(si => si.StageEmployee.EmployeeId == empId
                           && si.InterviewAssessment != null, ct);

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
