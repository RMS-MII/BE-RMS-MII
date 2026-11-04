using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class StageType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StageCategory StageCategory { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<BatchStage> BatchStages { get; set; } = [];
    public ICollection<QuestionBank> QuestionBanks { get; set; } = [];
}
