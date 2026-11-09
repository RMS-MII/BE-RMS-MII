namespace WEBAPI.Models;

public sealed class UserAccountDto
{
    public Guid AccountId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string EmployeeNumber { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public Guid DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmployeeIsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
