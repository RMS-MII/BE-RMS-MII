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

        var orderedStages = GetOrderedStages(stages);
        if (orderedStages.Count != stages.Count)
            return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Failure(
                Error.BusinessRule("The configured stage sequence is invalid. Reconfigure the batch stages."));

        return Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>.Success(
            orderedStages.Select(s => s.ToDtoWithInterviewers()).ToList());
    }

    private static IReadOnlyList<BatchStage> GetOrderedStages(
        IReadOnlyCollection<BatchStage> stages)
    {
        if (stages.Count == 0)
            return [];

        var stagesById = stages.ToDictionary(stage => stage.Id);
        var referencedStageIds = stages
            .Where(stage => stage.NextStageId.HasValue)
            .Select(stage => stage.NextStageId!.Value)
            .ToHashSet();
        var firstStages = stages
            .Where(stage => !referencedStageIds.Contains(stage.Id))
            .ToList();

        if (firstStages.Count != 1)
            return [];

        var orderedStages = new List<BatchStage>(stages.Count);
        var visitedIds = new HashSet<Guid>();
        var currentStage = firstStages[0];

        while (true)
        {
            if (!visitedIds.Add(currentStage.Id))
                return [];

            orderedStages.Add(currentStage);
            if (!currentStage.NextStageId.HasValue)
                break;

            if (!stagesById.TryGetValue(currentStage.NextStageId.Value, out var nextStage))
                return [];

            currentStage = nextStage;
        }

        return orderedStages.Count == stages.Count ? orderedStages : [];
    }
}
