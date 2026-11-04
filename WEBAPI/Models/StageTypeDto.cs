using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class StageTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StageCategory StageCategory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateStageTypeDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public StageCategory StageCategory { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateStageTypeDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public StageCategory StageCategory { get; init; }
    public bool IsActive { get; init; }
}
