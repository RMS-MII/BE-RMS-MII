using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Portal.PublicApplications;

public sealed record CandidateCodeRequest(string Email);
public sealed record CandidateCodeVerification(Guid VerificationId, string Code);

public sealed class PublicApplicationCodes(
    RmsDbContext db,
    IDateTimeProvider clock,
    IOptions<JwtSettings> jwt,
    IEmailService emailService,
    ILogger<PublicApplicationCodes> logger)
{
    private const int MaxAttempts = 5;

    public async Task<Result<Guid>> IssueAsync(string address, CancellationToken ct)
    {
        var email = address.Trim().ToLowerInvariant();
        if (email.Length > 100 || !MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
            return Result<Guid>.Failure(Error.Validation("Enter a valid email address."));

        var now = clock.GetUtcNow().UtcDateTime;
        var code = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var verification = new PublicApplicationVerification
        {
            Id = Guid.CreateVersion7(),
            Purpose = CandidateCodePurpose.Login,
            Email = email,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(10)
        };
        verification.CodeHash = Convert.ToHexString(Hash(verification.Id, email, code));

        await using (var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct))
        {
            var pastHour = now.AddHours(-1);
            var cooldown = now.AddMinutes(-1);
            var recent = await db.PublicApplicationVerifications
                .Where(item => item.Email == email && item.Purpose == CandidateCodePurpose.Login && item.CreatedAt >= pastHour)
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => item.CreatedAt)
                .Take(3)
                .ToListAsync(ct);
            if (recent.Count >= 3 || recent.Any(created => created >= cooldown))
                return Result<Guid>.Failure(new Error("VERIFICATION_RATE_LIMIT", "Please wait before requesting another code.", 429));

            await db.PublicApplicationVerifications
                .Where(item => item.Email == email && item.Purpose == CandidateCodePurpose.Login &&
                    item.UsedAt == null && item.ExpiresAt > now)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.ExpiresAt, now), ct);

            db.PublicApplicationVerifications.Add(verification);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        try
        {
            await emailService.SendVerificationEmailAsync(email, new EmailContent(
                "Your Rekrutin candidate sign-in code",
                $"Your verification code is {code}. It expires in 10 minutes. If you did not request this code, you can ignore this email."), ct);
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Candidate sign-in email delivery failed ({ExceptionType})", exception.GetType().Name);
            return Result<Guid>.Failure(new Error("VERIFICATION_DELIVERY_FAILED", "We couldnâ€™t send the verification code. Try again in a minute.", 503));
        }

        return Result<Guid>.Success(verification.Id);
    }

    public async Task<Result<string>> VerifyAsync(CandidateCodeVerification request, CancellationToken ct)
    {
        var invalidCode = Error.Validation("Enter a valid verification code or request a new one.");
        if (request.VerificationId == Guid.Empty || request.Code is not { Length: 6 } code ||
            code.Any(character => !char.IsAsciiDigit(character)))
            return Result<string>.Failure(invalidCode);

        var now = clock.GetUtcNow().UtcDateTime;
        var available = db.PublicApplicationVerifications.Where(item =>
            item.Id == request.VerificationId && item.Purpose == CandidateCodePurpose.Login &&
            item.UsedAt == null && item.ExpiresAt > now && item.FailedAttempts < MaxAttempts);
        var challenge = await available.AsNoTracking()
            .Select(item => new { item.Email, item.CodeHash }).SingleOrDefaultAsync(ct);
        if (challenge is null)
            return Result<string>.Failure(invalidCode);

        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(challenge.CodeHash),
                Hash(request.VerificationId, challenge.Email, code)))
        {
            await available.ExecuteUpdateAsync(update =>
                update.SetProperty(item => item.FailedAttempts, item => item.FailedAttempts + 1), ct);
            return Result<string>.Failure(invalidCode);
        }

        return await available.ExecuteUpdateAsync(update => update.SetProperty(item => item.UsedAt, now), ct) == 1
            ? Result<string>.Success(challenge.Email)
            : Result<string>.Failure(invalidCode);
    }

    private byte[] Hash(Guid verificationId, string email, string code) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwt.Value.Key),
            Encoding.UTF8.GetBytes($"rms-candidate-login:{verificationId:D}:{email}:{code}"));
}
