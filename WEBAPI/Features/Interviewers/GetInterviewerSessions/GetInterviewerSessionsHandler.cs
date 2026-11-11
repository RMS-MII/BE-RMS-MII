using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerSessions;

public sealed class GetInterviewerSessionsHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetInterviewerSessionsQuery, Result<IReadOnlyCollection<DashboardInterviewSessionDto>>>
{
    public async Task<Result<IReadOnlyCollection<DashboardInterviewSessionDto>>> Handle(
        GetInterviewerSessionsQuery request, CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId;
        if (employeeId is null)
        {
            return Result<IReadOnlyCollection<DashboardInterviewSessionDto>>.Failure(
                Error.Unauthorized("Autentikasi akun karyawan pewawancara diperlukan."));
        }

        var query = db.SessionInterviews
            .AsNoTracking()
            .Include(si => si.StageEmployee)
            .Include(si => si.InterviewSession)
                .ThenInclude(session => session.ApplicationStage)
                    .ThenInclude(appStage => appStage.BatchApplication)
                        .ThenInclude(ba => ba.Candidate)
            .Include(si => si.InterviewSession)
                .ThenInclude(session => session.ApplicationStage)
                    .ThenInclude(appStage => appStage.BatchStage)
                        .ThenInclude(batchStage => batchStage.RecruitmentBatch)
            .Include(si => si.InterviewAssessment)
            .Where(si => si.StageEmployee.EmployeeId == employeeId.Value)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(si => si.Status == request.Status.Value);
        }

        if (request.SessionStatus.HasValue)
        {
            query = query.Where(si => si.InterviewSession.Status == request.SessionStatus.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(si => si.InterviewSession.ScheduledStart >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(si => si.InterviewSession.ScheduledStart <= request.To.Value);
        }

        if (request.PendingActionOnly == true)
        {
            query = query.Where(si => si.InterviewSession.Status == InterviewSessionStatus.PendingConfirmation);
        }

        var sessions = await query
            .OrderBy(si => si.InterviewSession.ScheduledStart)
            .ToListAsync(ct);

        var sessionIds = sessions.Select(si => si.InterviewSessionId).Distinct().ToArray();
        var panelInterviewers = await db.SessionInterviews
            .AsNoTracking()
            .Include(panel => panel.StageEmployee)
                .ThenInclude(stageEmployee => stageEmployee.Employee)
            .Where(panel => sessionIds.Contains(panel.InterviewSessionId))
            .ToListAsync(ct);
        var panelsBySession = panelInterviewers
            .GroupBy(panel => panel.InterviewSessionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var session in sessions)
        {
            session.InterviewSession.SessionInterviews = panelsBySession.GetValueOrDefault(session.InterviewSessionId) ?? [];
        }

        var result = sessions
            .Select(si => si.ToDashboardInterviewSessionDto())
            .ToList();

        return Result<IReadOnlyCollection<DashboardInterviewSessionDto>>.Success(result);
    }
}
