using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class Candidate : BaseEntity
{
    public bool IsInTalentPool { get; set; } = true;
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Cv { get; set; }
    public string? Portfolio { get; set; }
    public string Education { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public decimal Gpa { get; set; }
    
    public ICollection<BatchApplication> BatchApplications { get; set; } = new List<BatchApplication>();
}
