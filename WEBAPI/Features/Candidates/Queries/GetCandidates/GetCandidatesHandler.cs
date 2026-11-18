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

        if (request.Gender.HasValue)
            query = query.Where(c => c.Gender == request.Gender.Value);

        switch (request.BatchStatus?.ToLowerInvariant())
        {
            case "applied":
                query = query.Where(c => c.BatchApplications.Any());
                break;
            case "pool":
                query = query.Where(c => !c.BatchApplications.Any());
                break;
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

        var descending = string.Equals(request.Order, "desc", StringComparison.OrdinalIgnoreCase);
        var sorted = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(c => c.FullName) : query.OrderBy(c => c.FullName),
            "education" => descending
                ? query.OrderByDescending(c => c.Education).ThenByDescending(c => c.Institution)
                : query.OrderBy(c => c.Education).ThenBy(c => c.Institution),
            "gpa" => descending ? query.OrderByDescending(c => c.Gpa) : query.OrderBy(c => c.Gpa),
            "salary" => descending ? query.OrderByDescending(c => c.ExpectedSalary) : query.OrderBy(c => c.ExpectedSalary),
            "batches" => descending ? query.OrderByDescending(c => c.BatchApplications.Count)
                : query.OrderBy(c => c.BatchApplications.Count),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var candidates = await sorted
            .ThenBy(c => c.Id)
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

