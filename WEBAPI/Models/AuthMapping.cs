using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class AuthMapping
{
    public static CurrentUserDto ToCurrentUserDto(this Account account) => new()
    {
        AccountId = account.Id,
        EmployeeId = account.EmployeeId,
        Username = account.Username,
        FullName = account.Employee.FullName,
        Email = account.Employee.Email,
        Role = account.Role.Name,
        DepartmentId = account.Employee.DepartmentId,
        DepartmentName = account.Employee.Department?.Name ?? string.Empty,
        JobTitle = account.Employee.JobTitle
    };

    public static LoginResponseDto ToLoginResponse(
        this Account account, string accessToken, DateTime expiresAt) => new()
    {
        AccessToken = accessToken,
        TokenType = "Bearer",
        ExpiresAt = expiresAt,
        User = account.ToCurrentUserDto()
    };
}
