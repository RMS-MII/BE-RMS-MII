using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler(
    RmsDbContext db,
    IEmailService emailService,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration,
    ILogger<ForgotPasswordHandler> logger) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();

        var account = await db.Accounts
            .AsNoTracking()
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.IsActive &&
                a.Employee.IsActive &&
                a.Employee.Email == email, ct);

        if (account is null)
            return Result.Success();

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        await db.PasswordResetTokens
            .Where(x => x.AccountId == account.Id && x.UsedAt == null)
            .ExecuteUpdateAsync(
                setter => setter.SetProperty(x => x.UsedAt, now), ct);

        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var tokenHast = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            AccountId = account.Id,
            TokenHash = tokenHast,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(30)
        });

        await db.SaveChangesAsync(ct);

        var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);
        var link = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        var content = EmailTemplates.PasswordReset(account.Employee.FullName, link);

        try
        {
            await emailService.SendPasswordResetEmailAsync(email, content, ct);
        }
        catch (Exception) when(!ct.IsCancellationRequested)
        {
            logger.LogWarning("Password reset email delivery failed for account {AccountId}", account.Id);
        }

        return Result.Success();
    }
}
