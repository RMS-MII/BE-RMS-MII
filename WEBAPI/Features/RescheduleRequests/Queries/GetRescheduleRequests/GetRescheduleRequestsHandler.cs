using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.RescheduleRequests.Queries.GetRescheduleRequests;

public class GetRescheduleRequestsHandler(RmsDbContext db)
    : IRequestHandler<GetRescheduleRequestsQuery, Result<IReadOnlyCollection<RescheduleRequestDto>>>
{
    public async Task<Result<IReadOnlyCollection<RescheduleRequestDto>>> Handle(
        GetRescheduleRequestsQuery query,
        CancellationToken ct)
    {
        var requestsQuery = db.RescheduleRequests.AsNoTracking().AsQueryable();

        if (query.Status.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.Status == query.Status.Value);
        }

        if (query.SessionId.HasValue)
        {
            requestsQuery = requestsQuery.Where(r => r.InterviewSessionId == query.SessionId.Value);
        }

        var requests = await requestsQuery
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        if (requests.Count == 0)
        {
            return Result<IReadOnlyCollection<RescheduleRequestDto>>.Success([]);
        }

        var sessionIds = requests.Select(r => r.InterviewSessionId).Distinct().ToList();

        var sessions = await db.InterviewSessions
            .AsNoTracking()
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Where(s => sessionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);

        var dtos = requests
            .Select(r => r.ToDto(sessions.GetValueOrDefault(r.InterviewSessionId)))
            .ToList();

        return Result<IReadOnlyCollection<RescheduleRequestDto>>.Success(dtos);
    }
}

