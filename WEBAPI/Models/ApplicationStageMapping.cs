using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class ApplicationStageMapping
{
    public static ApplicationStageDto ToDto(this ApplicationStage stage, BatchStage batchStage) => new()
    {
        Id = stage.Id,
        BatchApplicationId = stage.BatchApplicationId,
        BatchStageId = stage.BatchStageId,
        BatchStageName = batchStage.StageName,
        StageTypeId = batchStage.StageTypeId,
        StageTypeName = batchStage.StageType?.Name,
        StageCategory = batchStage.StageType?.StageCategory,
        PassingScore = batchStage.PassingScore,
        Status = stage.Status,
        TotalScore = stage.TotalScore,
        StartedAt = stage.StartedAt,
        EndAt = stage.EndAt,
        CompletedAt = stage.CompletedAt,
        Notes = stage.Notes,
        IsAttempts = stage.IsAttempts,
        DurationTest = stage.DurationTest,
        CreatedAt = stage.CreatedAt,
        UpdatedAt = stage.UpdatedAt
    };
}
