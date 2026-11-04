using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WEBAPI.Common.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? AccountId => ParseGuid(
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub));

    public Guid? EmployeeId => ParseGuid(User?.FindFirstValue(HrisClaimTypes.EmployeeId));

    public string? Role => User?.FindFirstValue(ClaimTypes.Role)
                           ?? User?.FindFirstValue(HrisClaimTypes.Role);

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    private static Guid? ParseGuid(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}
