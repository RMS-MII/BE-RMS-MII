using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    RmsDbContext db,
    IPasswordHasher<Account> passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim();

        var account = await db.Accounts
            .Include(a => a.Role)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(a => a.Username == username, ct);

        if (account is null || !account.IsActive)
        {
            logger.LogWarning("Login gagal: akun tidak ditemukan atau tidak aktif");
            return Result<LoginResponseDto>.Failure(
                Error.Unauthorized("Invalid username or password."));
        }

        var verification = passwordHasher.VerifyHashedPassword(
            account, account.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Login gagal: password salah untuk akun {AccountId}", account.Id);
            return Result<LoginResponseDto>.Failure(
                Error.Unauthorized("Invalid username or password."));
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            account.PasswordHash = passwordHasher.HashPassword(account, request.Password);

        account.LastLoginAt = dateTimeProvider.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);

        var (token, expiresAt) = jwtTokenService.GenerateToken(account);
        return Result<LoginResponseDto>.Success(account.ToLoginResponse(token, expiresAt));
    }
}
