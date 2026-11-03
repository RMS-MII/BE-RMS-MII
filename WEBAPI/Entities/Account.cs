namespace WEBAPI.Entities;

public class Account : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid RoleId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
