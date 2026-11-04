using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class DepartmentMapping
{
    public static DepartmentDto ToDto(this Department department) => new()
    {
        Id = department.Id,
        ParentDepartmentId = department.ParentDepartmentId,
        ParentDepartmentName = department.ParentDepartment?.Name,
        Code = department.Code,
        Name = department.Name,
        Description = department.Description,
        CreatedAt = department.CreatedAt,
        UpdatedAt = department.UpdatedAt
    };

    public static IReadOnlyCollection<DepartmentDto> ToDtoList(this IEnumerable<Department> departments)
        => departments.Select(ToDto).ToList();

    public static Department ToEntity(this CreateDepartmentDto dto) => new()
    {
        ParentDepartmentId = dto.ParentDepartmentId,
        Code = dto.Code.Trim(),
        Name = dto.Name.Trim(),
        Description = dto.Description?.Trim()
    };

    public static void ApplyTo(this UpdateDepartmentDto dto, Department department)
    {
        department.ParentDepartmentId = dto.ParentDepartmentId;
        department.Code = dto.Code.Trim();
        department.Name = dto.Name.Trim();
        department.Description = dto.Description?.Trim();
    }
}
