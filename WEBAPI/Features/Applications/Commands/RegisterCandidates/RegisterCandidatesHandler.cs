using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Portal;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Applications.Commands.RegisterCandidates;

public sealed class RegisterCandidatesHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<RegisterCandidatesHandler> logger)
    : IRequestHandler<RegisterCandidatesCommand, Result<IReadOnlyCollection<BatchApplicationDto>>>
{
    public async Task<Result<IReadOnlyCollection<BatchApplicationDto>>> Handle(
        RegisterCandidatesCommand request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
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

        var existingCandidates = await db.Candidates.AsNoTracking()
            .Where(candidate => emails.Contains(candidate.Email) || phoneNumbers.Contains(candidate.PhoneNumber))
            .ToListAsync(ct);
        var candidatesByEmail = existingCandidates
            .GroupBy(candidate => candidate.Email, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var candidatesByPhone = existingCandidates
            .GroupBy(candidate => candidate.PhoneNumber, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var newCandidates = new List<Candidate>();
        var resolvedCandidates = new List<ResolvedCandidate>(normalizedCandidates.Count);

        foreach (var item in normalizedCandidates)
        {
            candidatesByEmail.TryGetValue(item.Email, out var emailMatch);
            candidatesByPhone.TryGetValue(item.PhoneNumber, out var phoneMatch);

            if (emailMatch is not null && phoneMatch is not null && emailMatch.Id != phoneMatch.Id)
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
                candidatesByPhone[item.PhoneNumber] = candidate;
            }

            resolvedCandidates.Add(new ResolvedCandidate(candidate, item.Candidate.ExpectedSalary));
        }

        if (request.PublicCandidateId is { } publicCandidateId)
        {
            var candidate = await db.Candidates.AsNoTracking().SingleOrDefaultAsync(item => item.Id == publicCandidateId, ct);
            if (candidate is null)
                return Result<IReadOnlyCollection<BatchApplicationDto>>.Failure(
                    Error.BusinessRule("Complete your profile before applying."));
            resolvedCandidates.Add(new ResolvedCandidate(candidate, request.ExpectedSalary ?? 0));
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
            await db.Candidates.Where(candidate => existingCandidateIds.Contains(candidate.Id) && !candidate.IsInTalentPool)
                .ExecuteUpdateAsync(update => update.SetProperty(candidate => candidate.IsInTalentPool, true), ct);
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
