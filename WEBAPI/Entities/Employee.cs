using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class Employee : BaseEntity
{
    public Guid? ManagerId { get; set; }
    public Guid DepartmentId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = [];
    public Department Department { get; set; } = null!;
    public Account? Account { get; set; }
    public ICollection<StageEmployee> StageEmployees { get; set; } = [];
}
