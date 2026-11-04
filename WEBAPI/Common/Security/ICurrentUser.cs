namespace WEBAPI.Common.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? AccountId { get; }
    Guid? EmployeeId { get; }
    string? Role { get; }
    bool IsInRole(string role);
}
