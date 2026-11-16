using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Services;

namespace WEBAPI.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler(
    RmsDbContext db,
    IPasswordHasher<Account> passwordHasher,
    IDateTimeProvider dateTimeProvider,
    RefreshSessionService refreshSessions) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var resetToken = await db.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.UsedAt == null &&
                x.ExpiresAt > now, ct);

        if (resetToken is null)
            return Result.Failure(Error.Validation("Reset link is invalid or expired"));

        var account = await db.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == resetToken.AccountId &&
                a.IsActive &&
                a.Employee.IsActive, ct);

        if (account is null)
            return Result.Failure(Error.Validation("Reset link is invalid or expired"));

        var claimed = await db.PasswordResetTokens
            .Where(x =>
                x.Id == resetToken.Id &&
                x.UsedAt == null &&
                x.ExpiresAt > now)
            .ExecuteUpdateAsync(setter =>
                setter.SetProperty(x => x.UsedAt, now), ct);

        if (claimed != 1)
            return Result.Failure(Error.Validation("Reset link is invalid or expired"));

        account.PasswordHash = passwordHasher.HashPassword(account, request.NewPassword);
        account.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        await refreshSessions.RevokeAllForAccountAsync(account.Id, ct);
        await transaction.CommitAsync(ct);
        
        return Result.Success();
    }
}
