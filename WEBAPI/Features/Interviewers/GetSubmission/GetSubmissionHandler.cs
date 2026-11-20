using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetSubmission;

public class GetSubmissionHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetSubmissionQuery, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(
        GetSubmissionQuery query,
        CancellationToken ct)
    {
        var session = await db.InterviewSessions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchStage)
                    .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
            .FirstOrDefaultAsync(s => s.Id == query.InterviewSessionId, ct);

        if (session is null)
        {
            return Result<SubmissionDto>.Failure(
                Error.NotFound($"Interview session with ID '{query.InterviewSessionId}' was not found."));
        }

        // Validasi otorisasi: HR atau Pewawancara yang ditugaskan pada sesi ini
        var isHr = currentUser.IsInRole(RmsRoles.Hr);
        if (!isHr)
        {
            var currentEmployeeId = currentUser.EmployeeId;
            var isAssigned = currentEmployeeId.HasValue && session.SessionInterviews.Any(si =>
                si.StageEmployee.EmployeeId == currentEmployeeId.Value);

            if (!isAssigned)
            {
                return Result<SubmissionDto>.Failure(
                    Error.Forbidden("You do not have permission to view the submission for this interview session."));
            }
        }

        return Result<SubmissionDto>.Success(session.ToSubmissionDto());
    }
}
