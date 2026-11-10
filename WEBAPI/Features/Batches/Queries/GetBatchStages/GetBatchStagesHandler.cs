using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
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
            .ToListAsync(ct);

        var orderedStages = BatchStageSequence.GetOrderedStages(stages);
        if (orderedStages.Count != stages.Count)
            return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Failure(
                Error.BusinessRule("The configured stage sequence is invalid. Reconfigure the batch stages."));

        return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Success(
            orderedStages.Select(s => s.ToDtoWithInterviewers()).ToList());
    }

}
