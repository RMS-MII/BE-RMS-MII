using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesHandler(RmsDbContext db)
    : IRequestHandler<GetCandidatesQuery, Result<PagedResult<CandidateWithBatchesDto>>>
{
    public async Task<Result<PagedResult<CandidateWithBatchesDto>>> Handle(
        GetCandidatesQuery request, CancellationToken ct)
    {
        var query = db.Candidates
            .AsNoTracking()
            .Where(candidate => candidate.IsInTalentPool)
            .Include(c => c.BatchApplications)
                .ThenInclude(ba => ba.RecruitmentBatch)
            .AsQueryable();

        if (request.BatchId.HasValue)
        {
            query = query.Where(c => c.BatchApplications.Any(ba => ba.RecruitmentBatchId == request.BatchId.Value));
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.FullName.Contains(search) ||
                                     c.Email.Contains(search) ||
                                     c.PhoneNumber.Contains(search) ||
                                     c.Institution.Contains(search));
        }

        var totalItems = await query.CountAsync(ct);

        var candidates = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var result = new PagedResult<CandidateWithBatchesDto>(
            candidates.ToWithBatchesDtoList(),
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<CandidateWithBatchesDto>>.Success(result);
    }
}

