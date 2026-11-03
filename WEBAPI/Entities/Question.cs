using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class Question : BaseEntity
{
    public Guid QuestionBankId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;
    public decimal Weight { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public QuestionBank QuestionBank { get; set; } = null!;
    public ICollection<QuestionOptions> QuestionOptions { get; set; } = [];
}
