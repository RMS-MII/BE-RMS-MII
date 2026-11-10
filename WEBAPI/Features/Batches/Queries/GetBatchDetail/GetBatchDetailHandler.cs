using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDetail;

public sealed class GetBatchDetailHandler(RmsDbContext db)
    : IRequestHandler<GetBatchDetailQuery, Result<RecruitmentBatchDto>>
{
    public async Task<Result<RecruitmentBatchDto>> Handle(
        GetBatchDetailQuery request, CancellationToken ct)
    {
        var batch = await db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .Include(b => b.BatchStages)
                .ThenInclude(s => s.StageType)
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (batch is null)
            return Result<RecruitmentBatchDto>.Failure(
                Error.NotFound("Batch was not found."));

        var dto = batch.ToDto();
        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
