using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Portal;
using WEBAPI.Features.Portal.PublicApplications;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Applications.Commands.RegisterCandidates;

public sealed class RegisterCandidatesHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<RegisterCandidatesHandler> logger,
    PublicApplicationCodes codes)
    : IRequestHandler<RegisterCandidatesCommand, Result<IReadOnlyCollection<BatchApplicationDto>>>
{
    public async Task<Result<IReadOnlyCollection<BatchApplicationDto>>> Handle(
        RegisterCandidatesCommand request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        if (request.IsPublicApplication && (request.Candidates.Count != 1 ||
            !await codes.TryConsumeAsync(request, now, ct)))
        {
            await transaction.CommitAsync(ct);
            return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                Error.Validation("Enter a valid verification code or request a new one."));
        }

        var batches = db.RecruitmentBatches.AsNoTracking();
        if (request.IsPublicApplication)
            batches = batches.Where(PublicJobAvailability.AcceptsApplications(now));

        var batch = await batches
            .Include(item => item.BatchStages)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, ct);

        if (batch is null)
            return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                Error.NotFound(request.IsPublicApplication
                    ? "This position is no longer accepting applications."
                    : "Batch was not found."));

        if (batch.Status != BatchStatus.Open)
            return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                Error.BusinessRule("Open the recruitment batch before registering candidates."));

        var isStandalone = batch.BatchStages.Any(stage =>
            stage.LifecycleStatus != BatchStageLifecycleStatus.Inactive);

        var normalizedCandidates = request.Candidates
            .Select(candidate => new NormalizedCandidate(
                candidate,
                candidate.Email.Trim(),
                candidate.PhoneNumber.Trim()))
            .ToList();

        var emails = normalizedCandidates
            .Select(candidate => candidate.Email)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var phoneNumbers = normalizedCandidates
            .Select(candidate => candidate.PhoneNumber)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var candidatesQuery = db.Candidates.AsNoTracking();
        if (request.IsPublicApplication)
        {
            var phone = NormalizePhone(phoneNumbers.Single());
            var localPhone = phone.StartsWith("628", StringComparison.Ordinal) ? "0" + phone[2..] : phone;
            candidatesQuery = candidatesQuery.Where(candidate =>
                emails.Contains(candidate.Email) ||
                new[] { phone, localPhone }.Contains(candidate.PhoneNumber
                    .Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")));
        }
        else
        {
            candidatesQuery = candidatesQuery.Where(candidate =>
                emails.Contains(candidate.Email) || phoneNumbers.Contains(candidate.PhoneNumber));
        }

        var existingCandidates = await candidatesQuery.ToListAsync(ct);
        var candidatesByEmail = existingCandidates
            .GroupBy(candidate => candidate.Email, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var candidatesByPhone = existingCandidates
            .GroupBy(candidate => request.IsPublicApplication ? NormalizePhone(candidate.PhoneNumber) : candidate.PhoneNumber,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var newCandidates = new List<Candidate>();
        var resolvedCandidates = new List<ResolvedCandidate>(normalizedCandidates.Count);

        foreach (var item in normalizedCandidates)
        {
            candidatesByEmail.TryGetValue(item.Email, out var emailMatch);
            var phoneKey = request.IsPublicApplication ? NormalizePhone(item.PhoneNumber) : item.PhoneNumber;
            candidatesByPhone.TryGetValue(phoneKey, out var phoneMatch);

            if (request.IsPublicApplication)
            {
                // OTP identifies the email owner even when a legacy profile shares its formatted phone number.
                var contactConflict = emailMatch is null
                    ? phoneMatch is not null
                    : NormalizePhone(emailMatch.PhoneNumber) != phoneKey;
                if (contactConflict)
                    return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                        Error.Conflict("Unable to register with these contact details. Use the phone number on your candidate profile or contact recruitment for help."));
            }
            else if (emailMatch is not null && phoneMatch is not null && emailMatch.Id != phoneMatch.Id)
                return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                    Error.Conflict(
                        $"Email {item.Email} and phone number {item.PhoneNumber} belong to different candidates."));

            var candidate = emailMatch ?? phoneMatch;

            if (candidate is null)
            {
                candidate = (item.Candidate with { ExpectedSalary = 0 }).ToEntity();
                candidate.Id = Guid.CreateVersion7();
                candidate.CreatedAt = now;

                newCandidates.Add(candidate);

                // Keep the lookup up to date so a repeated identity in the same request resolves
                // to the same in-memory candidate instead of creating a duplicate row.
                candidatesByEmail[item.Email] = candidate;
                candidatesByPhone[phoneKey] = candidate;
            }

            resolvedCandidates.Add(new ResolvedCandidate(candidate, item.Candidate.ExpectedSalary));
        }

        var duplicateCandidate = resolvedCandidates
            .GroupBy(item => item.Candidate.Id)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateCandidate is not null)
            return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                Error.Conflict("The same candidate cannot be registered more than once in the same request."));

        var newCandidateIds = newCandidates
            .Select(candidate => candidate.Id)
            .ToHashSet();

        var existingCandidateIds = resolvedCandidates
            .Select(item => item.Candidate.Id)
            .Where(id => !newCandidateIds.Contains(id))
            .Distinct()
            .ToList();

        if (existingCandidateIds.Count > 0)
        {
            var alreadyRegisteredIds = await db.BatchApplications
                .AsNoTracking()
                .Where(application => application.RecruitmentBatchId == batch.Id &&
                                      existingCandidateIds.Contains(application.CandidateId))
                .Select(application => application.CandidateId)
                .ToListAsync(ct);

            if (alreadyRegisteredIds.Count > 0)
            {
                if (request.IsPublicApplication)
                    return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                        Error.Conflict("An application has already been submitted for this position."));

                var registeredIds = alreadyRegisteredIds.ToHashSet();
                var names = resolvedCandidates
                    .Where(item => registeredIds.Contains(item.Candidate.Id))
                    .Select(item => item.Candidate.FullName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                    Error.Conflict(
                        $"Candidate(s) already registered in this batch: {string.Join(", ", names)}."));
            }
        }

        var applications = resolvedCandidates
            .Select(item => new BatchApplication
            {
                Id = Guid.CreateVersion7(),
                CandidateId = item.Candidate.Id,
                RecruitmentBatchId = batch.Id,
                IsStandalone = isStandalone,
                ExpectedSalary = item.ExpectedSalary > 0 ? item.ExpectedSalary : null,
                CodeApplication = GenerateCode(now),
                AppliedAt = now,
                Status = ApplicationStatus.InProcess,
                OfferingLetterStatus = OfferingLetterStatus.Pending,
                Notes = string.Empty,
                FileOfferingLetter = string.Empty,
                CreatedAt = now
            })
            .ToList();

        db.Candidates.AddRange(newCandidates);
        db.BatchApplications.AddRange(applications);

        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            // Unique index tetap menjadi penentu akhir jika ada request lain yang berjalan bersamaan.
            logger.LogWarning(
                "Register candidates gagal: kandidat duplikat pada batch {BatchId}", batch.Id);

            return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                Error.Conflict("One or more candidates are already registered in this batch."));
        }

        var result = applications
            .Select((application, index) => application.ToDto(resolvedCandidates[index].Candidate))
            .ToList();

        return Result<IReadOnlyCollection<BatchApplicationDto>>.Success(result);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return digits.StartsWith("08", StringComparison.Ordinal) ? "62" + digits[1..] : digits;
    }

    private static string GenerateCode(DateTime now)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"APP-{now:yyyyMMdd}-{suffix}";
    }

    private sealed record NormalizedCandidate(
        CreateCandidateDto Candidate,
        string Email,
        string PhoneNumber);

    private sealed record ResolvedCandidate(Candidate Candidate, decimal ExpectedSalary);
}
