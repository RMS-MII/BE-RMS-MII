using WEBAPI.Entities;

namespace WEBAPI.Features.Batches;

internal static class BatchStageSequence
{
    public static IReadOnlyList<BatchStage> GetOrderedStages(
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
