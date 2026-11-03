namespace WEBAPI.Entities;

public class Department : BaseEntity
{
    public Guid? ParentDepartmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = [];
    public ICollection<Employee> Employees { get; set; } = [];
    public ICollection<QuestionBank> QuestionBanks { get; set; } = [];
}
