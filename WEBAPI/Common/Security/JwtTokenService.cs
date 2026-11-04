using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WEBAPI.Entities;
using WEBAPI.Services;

namespace WEBAPI.Common.Security;

public sealed class JwtTokenService(
    IOptions<JwtSettings> settings,
    IDateTimeProvider dateTimeProvider) : IJwtTokenService
{
    private readonly JwtSettings _settings = settings.Value;

    public (string Token, DateTime ExpiresAt) GenerateToken(Account account)
    {
        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(_settings.ExpirateInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(HrisClaimTypes.EmployeeId, account.EmployeeId.ToString()),
            new(HrisClaimTypes.Role, account.Role.Name),
            new(HrisClaimTypes.DepartmentId, account.Employee.DepartmentId.ToString()),
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Username)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
