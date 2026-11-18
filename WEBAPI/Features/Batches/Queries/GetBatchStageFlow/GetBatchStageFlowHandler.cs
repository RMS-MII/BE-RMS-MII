using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchStageFlow;

public sealed class GetBatchStageFlowHandler(RmsDbContext db)
    : IRequestHandler<GetBatchStageFlowQuery, Result<IReadOnlyCollection<BatchStageFlowDto>>>
{
    public async Task<Result<IReadOnlyCollection<BatchStageFlowDto>>> Handle(
        GetBatchStageFlowQuery request, CancellationToken ct)
    {
        if (!await db.RecruitmentBatches.AsNoTracking()
                .AnyAsync(batch => batch.Id == request.BatchId, ct))
            return Result<IReadOnlyCollection<BatchStageFlowDto>>.Failure(
                Error.NotFound("Batch was not found."));

        var stages = await db.BatchStages.AsNoTracking()
            .Include(stage => stage.StageType)
            .Where(stage => stage.RecruitmentBatchId == request.BatchId)
            .ToListAsync(ct);
        var orderedStages = BatchStageSequence.GetOrderedStages(stages);
        if (orderedStages.Count != stages.Count)
            return Result<IReadOnlyCollection<BatchStageFlowDto>>.Failure(
                Error.BusinessRule("The configured stage sequence is invalid. Reconfigure the batch stages."));

        var counts = await db.ApplicationStages.AsNoTracking()
            .Where(applicationStage => applicationStage.BatchStage.RecruitmentBatchId == request.BatchId)
            .GroupBy(applicationStage => applicationStage.BatchStageId)
            .Select(group => new { StageId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.StageId, item => item.Count, ct);

        var result = orderedStages.Select(stage => new BatchStageFlowDto
        {
            Id = stage.Id,
            StageTypeId = stage.StageTypeId,
            StageName = stage.StageName,
            StageCategory = stage.StageType.StageCategory,
            LifecycleStatus = stage.LifecycleStatus,
            CandidateCount = counts.GetValueOrDefault(stage.Id),
            PassingScore = stage.PassingScore,
            EndDate = stage.EndDate
        }).ToList();
        return Result<IReadOnlyCollection<BatchStageFlowDto>>.Success(result);
    }
}
