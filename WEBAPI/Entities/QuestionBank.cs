namespace WEBAPI.Entities;

public class QuestionBank : BaseEntity
{
    public Guid DepartmentId { get; set; }
    public Guid StageTypeId { get; set; }
    public Guid CreatedBy { get; set; }
    public int DurationInMinutes {get; set;}

    // Navigation properties
    public Department Department { get; set; } = null!;
    public StageType StageType { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = [];
}
