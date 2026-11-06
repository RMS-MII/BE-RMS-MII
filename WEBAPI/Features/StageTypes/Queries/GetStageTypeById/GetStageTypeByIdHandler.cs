using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.StageTypes.Queries.GetStageTypeById;

public sealed class GetStageTypeByIdHandler(RmsDbContext db)
    : IRequestHandler<GetStageTypeByIdQuery, Result<StageTypeDto>>
{
    public async Task<Result<StageTypeDto>> Handle(
        GetStageTypeByIdQuery request, CancellationToken ct)
    {
        var stageType = await db.StageTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == request.Id, ct);

        if (stageType is null)
        {
            return Result<StageTypeDto>.Failure(
                Error.NotFound($"Tipe tahapan dengan ID '{request.Id}' tidak ditemukan."));
        }

        return Result<StageTypeDto>.Success(stageType.ToDto());
    }
}

