using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.StageTypes.Queries.GetStageTypes;

public sealed class GetStageTypesHandler(RmsDbContext db)
    : IRequestHandler<GetStageTypesQuery, Result<IReadOnlyCollection<StageTypeDto>>>
{
    public async Task<Result<IReadOnlyCollection<StageTypeDto>>> Handle(
        GetStageTypesQuery request, CancellationToken ct)
    {
        var query = db.StageTypes
            .AsNoTracking()
            .AsQueryable();

        if (request.Category.HasValue)
        {
            query = query.Where(st => st.StageCategory == request.Category.Value);
        }

        if (request.ActiveOnly == true)
        {
            query = query.Where(st => st.IsActive);
        }

        var stageTypes = await query
            .OrderBy(st => st.StageCategory)
            .ThenBy(st => st.Name)
            .ToListAsync(ct);

        return Result<IReadOnlyCollection<StageTypeDto>>.Success(stageTypes.ToDtoList());
    }
}

