using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatches;

public sealed class GetBatchesHandler(RmsDbContext db)
    : IRequestHandler<GetBatchesQuery, Result<PagedResult<RecruitmentBatchDto>>>
{
    public async Task<Result<PagedResult<RecruitmentBatchDto>>> Handle(
        GetBatchesQuery request, CancellationToken ct)
    {
        var query = db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(b => b.DepartmentId == request.DepartmentId.Value);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.BatchCode.Contains(search) || b.Name.Contains(search));

        var totalItems = await query.CountAsync(ct);

        var batches = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var result = new PagedResult<RecruitmentBatchDto>(
            batches.ToDtoList(),
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<RecruitmentBatchDto>>.Success(result);
    }
}
