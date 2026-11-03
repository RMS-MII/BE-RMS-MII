using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class StageType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StageCategory StageCategory { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<BatchStage> BatchStages { get; set; } = [];
}
