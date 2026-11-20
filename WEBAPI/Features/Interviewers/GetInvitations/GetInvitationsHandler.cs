using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInvitations;

public class GetInvitationsHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetInvitationsQuery, Result<List<InterviewerInvitationDto>>>
{
    public async Task<Result<List<InterviewerInvitationDto>>> Handle(
        GetInvitationsQuery query,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId;
        if (!employeeId.HasValue)
        {
            return Result<List<InterviewerInvitationDto>>.Failure(
                Error.Unauthorized("Your account is not linked to an employee profile."));
        }

        var dbQuery = db.StageEmployees
            .AsNoTracking()
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.StageType)
            .Where(se => se.EmployeeId == employeeId.Value && se.IsActive);

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(se => se.Status == query.Status.Value);
        }

        var invitations = await dbQuery
            .OrderByDescending(se => se.CreatedAt)
            .ToListAsync(ct);

        return Result<List<InterviewerInvitationDto>>.Success(invitations.ToInterviewerInvitationDtoList());
    }
}
