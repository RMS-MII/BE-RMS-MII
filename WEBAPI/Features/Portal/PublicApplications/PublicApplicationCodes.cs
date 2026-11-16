using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Features.Applications.Commands.RegisterCandidates;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Portal.PublicApplications;

public sealed record VerifyPublicApplicationRequest(CreateCandidateDto Candidate, Guid VerificationId, string Code);

public sealed class PublicApplicationCodes(
    RmsDbContext db,
    IDateTimeProvider clock,
    IOptions<JwtSettings> jwt,
    IEmailService emailService,
    ILogger<PublicApplicationCodes> logger)
{
    private const int MaxAttempts = 5;

    public async Task<Result<Guid>> IssueAsync(Guid batchId, CreateCandidateDto candidate, CancellationToken ct)
    {
        var validation = await new RegisterCandidatesValidator().ValidateAsync(new RegisterCandidatesCommand
        {
            BatchId = batchId,
            IsPublicApplication = true,
            Candidates = [candidate]
        }, ct);
        if (!validation.IsValid)
            return Result<Guid>.Failure(Error.Validation(validation.Errors[0].ErrorMessage));

        var now = clock.GetUtcNow().UtcDateTime;
        var email = NormalizeEmail(candidate.Email);
        if (!await db.RecruitmentBatches.Where(PublicJobAvailability.AcceptsApplications(now))
                .AnyAsync(batch => batch.Id == batchId, ct))
            return Result<Guid>.Failure(Error.NotFound("This position is no longer accepting applications."));

        var code = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var verification = new PublicApplicationVerification
        {
            Id = Guid.CreateVersion7(),
            BatchId = batchId,
            Email = email,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(10)
        };
        verification.CodeHash = Convert.ToHexString(Hash(verification.Id, batchId, candidate, code));

        await using (var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct))
        {
            var pastHour = now.AddHours(-1);
            var cooldown = now.AddMinutes(-1);
            var recent = await db.PublicApplicationVerifications
                .Where(item => item.Email == email && item.CreatedAt >= pastHour)
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => item.CreatedAt)
                .Take(3)
                .ToListAsync(ct);
            if (recent.Count >= 3 || recent.Any(created => created >= cooldown))
                return Result<Guid>.Failure(new Error("VERIFICATION_RATE_LIMIT", "Please wait before requesting another code.", 429));

            await db.PublicApplicationVerifications
                .Where(item => item.Email == email && item.BatchId == batchId &&
                    item.UsedAt == null && item.ExpiresAt > now)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.ExpiresAt, now), ct);

            db.PublicApplicationVerifications.Add(verification);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        try
        {
            await emailService.SendVerificationEmailAsync(email, new EmailContent(
                "Your RMS application verification code",
                $"Your verification code is {code}. It expires in 10 minutes. If you did not request this code, you can ignore this email."), ct);
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Application verification email delivery failed ({ExceptionType})", exception.GetType().Name);
            return Result<Guid>.Failure(new Error("VERIFICATION_DELIVERY_FAILED", "We couldn’t send the verification code. Try again in a minute.", 503));
        }

        return Result<Guid>.Success(verification.Id);
    }

    // The registration handler owns the transaction so consuming a code and creating the application commit together.
    public async Task<bool> TryConsumeAsync(RegisterCandidatesCommand request, DateTime now, CancellationToken ct)
    {
        if (request.VerificationId == Guid.Empty || request.VerificationCode is not { Length: 6 } code ||
            code.Any(character => !char.IsAsciiDigit(character)))
            return false;

        var candidate = request.Candidates.Single();
        var email = NormalizeEmail(candidate.Email);
        var available = db.PublicApplicationVerifications.Where(item =>
            item.Id == request.VerificationId && item.BatchId == request.BatchId && item.Email == email &&
            item.UsedAt == null && item.ExpiresAt > now && item.FailedAttempts < MaxAttempts);
        var storedHash = await available.Select(item => item.CodeHash).SingleOrDefaultAsync(ct);
        if (storedHash is null)
            return false;

        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(storedHash),
                Hash(request.VerificationId, request.BatchId, candidate, code)))
        {
            await available.ExecuteUpdateAsync(update =>
                update.SetProperty(item => item.FailedAttempts, item => item.FailedAttempts + 1), ct);
            return false;
        }

        return await available.ExecuteUpdateAsync(update => update.SetProperty(item => item.UsedAt, now), ct) == 1;
    }

    private byte[] Hash(Guid verificationId, Guid batchId, CreateCandidateDto candidate, string code) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwt.Value.Key),
            JsonSerializer.SerializeToUtf8Bytes(new
            {
                Purpose = "rms-public-application",
                VerificationId = verificationId,
                BatchId = batchId,
                Candidate = candidate with { Email = NormalizeEmail(candidate.Email) },
                Code = code
            }));

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
