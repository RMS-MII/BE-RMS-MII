using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchStages;

public sealed class GetBatchStagesHandler(RmsDbContext db)
    : IRequestHandler<GetBatchStagesQuery, Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>>
{
    public async Task<Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>> Handle(
        GetBatchStagesQuery request, CancellationToken ct)
    {
        var batchExists = await db.RecruitmentBatches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BatchId, ct);

        if (!batchExists)
            return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Failure(
                Error.NotFound("Batch was not found."));

        var stages = await db.BatchStages
            .AsNoTracking()
            .Include(s => s.StageType)
            .Include(s => s.StageEmployees)
                .ThenInclude(se => se.Employee)
                    .ThenInclude(e => e.Department)
            .Where(s => s.RecruitmentBatchId == request.BatchId)
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .ToListAsync(ct);

        return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Success(
            stages.Select(s => s.ToDtoWithInterviewers()).ToList());
    }
}
