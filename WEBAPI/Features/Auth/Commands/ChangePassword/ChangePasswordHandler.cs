using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Services;

namespace WEBAPI.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher<Account> passwordHasher,
    IDateTimeProvider dateTimeProvider,
    RefreshSessionService refreshSessions)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (currentUser.AccountId is not { } accountId)
        {
            return Result.Failure(Error.Unauthorized("Authentication is required"));
        }

        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.IsActive, ct);

        if (account is null)
        {
            return Result.Failure(Error.Unauthorized("Account is unavailable"));
        }

        var verification = passwordHasher.VerifyHashedPassword(
            account,
            account.PasswordHash,
            request.CurrentPassword);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Result.Failure(Error.Validation("Current password is incorrect"));
        }

        account.PasswordHash = passwordHasher.HashPassword(
            account,
            request.NewPassword);

        account.UpdatedAt = dateTimeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct);
        await refreshSessions.RevokeAllForAccountAsync(accountId, ct);
        await transaction.CommitAsync(ct);
        return Result.Success();

    }
}
