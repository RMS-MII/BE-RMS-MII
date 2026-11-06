using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class EmployeeMapping
{
    public static EmployeeDto ToDto(this Employee employee) => new()
    {
        Id = employee.Id,
        ManagerId = employee.ManagerId,
        ManagerName = employee.Manager?.FullName,
        DepartmentId = employee.DepartmentId,
        DepartmentName = employee.Department?.Name ?? string.Empty,
        EmployeeNumber = employee.EmployeeNumber,
        FullName = employee.FullName,
        Email = employee.Email,
        Gender = employee.Gender,
        JobTitle = employee.JobTitle,
        IsActive = employee.IsActive,
        Role = employee.Account?.Role?.Name,
        CreatedAt = employee.CreatedAt,
        UpdatedAt = employee.UpdatedAt
    };

    public static IReadOnlyCollection<EmployeeDto> ToDtoList(this IEnumerable<Employee> employees)
        => employees.Select(ToDto).ToList();
}

