namespace WEBAPI.Models;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateDepartmentDto
{
    public Guid? ParentDepartmentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateDepartmentDto
{
    public Guid? ParentDepartmentId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
