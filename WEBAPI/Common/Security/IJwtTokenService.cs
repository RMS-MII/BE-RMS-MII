using WEBAPI.Entities;

namespace WEBAPI.Common.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Account account);
}
