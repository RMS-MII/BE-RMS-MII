using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class CandidateDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Cv { get; set; }
    public string? Portfolio { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public decimal Gpa { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateCandidateDto
{
    public string FullName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string? Cv { get; init; }
    public string? Portfolio { get; init; }
    public string Institution { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal ExpectedSalary { get; init; }
    public decimal Gpa { get; init; }
}

public record UpdateCandidateDto
{
    public string FullName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string? Cv { get; init; }
    public string? Portfolio { get; init; }
    public string Institution { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal ExpectedSalary { get; init; }
    public decimal Gpa { get; init; }
}
