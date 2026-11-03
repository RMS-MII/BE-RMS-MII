namespace WEBAPI.Entities;

public class QuestionBank : BaseEntity
{
    public Guid DepartmentId { get; set; }
    public Guid StageTypeId { get; set; }
    public Guid CreatedBy { get; set; }

    // Navigation property (Question -> QuestionOptions)
    public ICollection<Question> Questions { get; set; } = [];
}
