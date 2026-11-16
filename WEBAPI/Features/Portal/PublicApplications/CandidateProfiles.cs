using System.Data;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Features.Applications.Commands.RegisterCandidates;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.PublicApplications;

public sealed class CandidateProfiles(RmsDbContext db, IDateTimeProvider clock)
{
    public async Task<Result<CandidateDto>> SaveAsync(string email, CreateCandidateDto profile, CancellationToken ct)
    {
        profile = profile with { Email = email, ExpectedSalary = 0 };
        var validation = await new RegisterCandidateValidator().ValidateAsync(profile, ct);
        if (!validation.IsValid)
            return Result<CandidateDto>.Failure(Error.Validation(validation.Errors[0].ErrorMessage));
        if (!RegisterCandidatesValidator.IsOptionalWebUrl(profile.Cv) ||
            !RegisterCandidatesValidator.IsOptionalWebUrl(profile.Portfolio))
            return Result<CandidateDto>.Failure(Error.Validation("Enter valid HTTP or HTTPS document links."));

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var candidate = await db.Candidates.SingleOrDefaultAsync(item => item.Email == email, ct);
        var phone = NormalizePhone(profile.PhoneNumber);
        var existingPhone = candidate is null ? null : NormalizePhone(candidate.PhoneNumber);
        if (existingPhone != phone)
        {
            var id = candidate?.Id;
            var localPhone = phone.StartsWith("628", StringComparison.Ordinal) ? "0" + phone[2..] : phone;
            var aliases = new[] { phone, localPhone };
            if (await db.Candidates.AnyAsync(item => item.Id != id && aliases.Contains(item.PhoneNumber
                    .Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")), ct))
                return Result<CandidateDto>.Failure(PhoneConflict());
        }

        var now = clock.GetUtcNow().UtcDateTime;
        var normalized = profile.ToEntity();
        if (candidate is null)
        {
            candidate = normalized;
            candidate.Id = Guid.CreateVersion7();
            candidate.CreatedAt = now;
            candidate.IsInTalentPool = false;
            db.Candidates.Add(candidate);
        }
        else
        {
            candidate.FullName = normalized.FullName;
            candidate.Gender = normalized.Gender;
            candidate.BirthDate = normalized.BirthDate;
            candidate.PhoneNumber = existingPhone == phone ? candidate.PhoneNumber : normalized.PhoneNumber;
            candidate.Education = normalized.Education;
            candidate.Institution = normalized.Institution;
            candidate.Address = normalized.Address;
            candidate.Cv = normalized.Cv;
            candidate.Portfolio = normalized.Portfolio;
            candidate.Gpa = normalized.Gpa;
            candidate.UpdatedAt = now;
        }

        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.IsUniqueConstraintViolation())
        {
            return Result<CandidateDto>.Failure(Error.Conflict("The contact details changed. Refresh and try again."));
        }

        return Result<CandidateDto>.Success(candidate.ToDto());
    }

    private static Error PhoneConflict() =>
        Error.Conflict("This phone number is already in use. Contact recruitment for help.");

    private static string NormalizePhone(string phone)
    {
        var digits = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return digits.StartsWith("08", StringComparison.Ordinal) ? "62" + digits[1..] : digits;
    }
}
