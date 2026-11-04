using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class StageTypeMapping
{
    public static StageTypeDto ToDto(this StageType stageType) => new()
    {
        Id = stageType.Id,
        Name = stageType.Name,
        Description = stageType.Description,
        StageCategory = stageType.StageCategory,
        IsActive = stageType.IsActive,
        CreatedAt = stageType.CreatedAt,
        UpdatedAt = stageType.UpdatedAt
    };

    public static IReadOnlyCollection<StageTypeDto> ToDtoList(this IEnumerable<StageType> stageTypes)
        => stageTypes.Select(ToDto).ToList();

    public static StageType ToEntity(this CreateStageTypeDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Description = dto.Description?.Trim(),
        StageCategory = dto.StageCategory,
        IsActive = dto.IsActive
    };

    public static void ApplyTo(this UpdateStageTypeDto dto, StageType stageType)
    {
        stageType.Name = dto.Name.Trim();
        stageType.Description = dto.Description?.Trim();
        stageType.StageCategory = dto.StageCategory;
        stageType.IsActive = dto.IsActive;
    }
}
