namespace WEBAPI.Entities;

public class QuestionOptions : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string OptionLabel { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public Question Question { get; set; } = null!;
}
