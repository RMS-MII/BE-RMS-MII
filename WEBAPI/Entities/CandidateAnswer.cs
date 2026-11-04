namespace WEBAPI.Entities;

public class CandidateAnswer : BaseEntity
{
    public Guid ApplicationStageId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionOptionText { get; set; } = string.Empty;
    public decimal Score { get; set; }

    // Navigation properties
    public ApplicationStage ApplicationStage { get; set; } = null!;
}
