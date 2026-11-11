using Microsoft.EntityFrameworkCore;
using Serilog;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Seeders;

public static class SampleBatchApplicationsSeeder
{
    private static readonly (string Name, string Email, Gender Gender, ApplicationStatus Status,
        ApplicationStageStatus StageStatus)[] Samples =
    [
        ("Alya Pratama", "seed.alya@example.com", Gender.Female,
            ApplicationStatus.InProcess, ApplicationStageStatus.InProgress),
        ("Bima Saputra", "seed.bima@example.com", Gender.Male,
            ApplicationStatus.InProcess, ApplicationStageStatus.InProgress),
        ("Citra Lestari", "seed.citra@example.com", Gender.Female,
            ApplicationStatus.Rejected, ApplicationStageStatus.Rejected),
        ("Damar Wijaya", "seed.damar@example.com", Gender.Male,
            ApplicationStatus.Dropped, ApplicationStageStatus.Dropped)
    ];

    public static async Task SeedAsync(RmsDbContext context, CancellationToken ct = default)
    {
        var activeStages = await context.BatchStages
            .AsNoTracking()
            .Where(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active
                && stage.RecruitmentBatch.Status == BatchStatus.Open)
            .OrderBy(stage => stage.StartDate)
            .ToListAsync(ct);
        var stagesByBatch = activeStages
            .GroupBy(stage => stage.RecruitmentBatchId)
            .Select(group => group.First())
            .ToList();

        if (stagesByBatch.Count == 0)
        {
            Log.Warning("Sample applications skipped: no open recruitment batch has an active stage.");
            return;
        }

        var emails = Samples.Select(sample => sample.Email).ToArray();
        var existingCandidates = await context.Candidates
            .Where(candidate => emails.Contains(candidate.Email))
            .ToListAsync(ct);
        var candidatesByEmail = existingCandidates.ToDictionary(
            candidate => candidate.Email, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var sample in Samples)
        {
            if (candidatesByEmail.ContainsKey(sample.Email))
                continue;

            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FullName = sample.Name,
                Email = sample.Email,
                Gender = sample.Gender,
                PhoneNumber = "081234567890",
                BirthDate = new DateOnly(2000, 1, 1),
                Education = "S1",
                Institution = "Universitas Contoh",
                Address = "Jakarta",
                ExpectedSalary = 8000000m,
                Gpa = 3.5m,
                CreatedAt = now
            };
            context.Candidates.Add(candidate);
            candidatesByEmail.Add(sample.Email, candidate);
        }

        await context.SaveChangesAsync(ct);

        var batchIds = stagesByBatch.Select(stage => stage.RecruitmentBatchId).ToArray();
        var candidateIds = candidatesByEmail.Values.Select(candidate => candidate.Id).ToArray();
        var existingApplications = await context.BatchApplications
            .AsNoTracking()
            .Where(application => batchIds.Contains(application.RecruitmentBatchId)
                && candidateIds.Contains(application.CandidateId))
            .Select(application => new { application.RecruitmentBatchId, application.CandidateId })
            .ToListAsync(ct);
        var existingPairs = existingApplications
            .Select(application => (application.RecruitmentBatchId, application.CandidateId))
            .ToHashSet();

        var added = 0;
        foreach (var stage in stagesByBatch)
        {
            for (var index = 0; index < Samples.Length; index++)
            {
                var sample = Samples[index];
                var candidate = candidatesByEmail[sample.Email];
                if (!existingPairs.Add((stage.RecruitmentBatchId, candidate.Id)))
                    continue;

                var application = new BatchApplication
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidate.Id,
                    RecruitmentBatchId = stage.RecruitmentBatchId,
                    CodeApplication = $"SEED-{stage.RecruitmentBatchId:N}-{index + 1:00}",
                    AppliedAt = now.AddDays(-2),
                    Status = sample.Status,
                    CreatedAt = now
                };
                context.BatchApplications.Add(application);
                context.ApplicationStages.Add(new ApplicationStage
                {
                    Id = Guid.NewGuid(),
                    BatchApplicationId = application.Id,
                    BatchStageId = stage.Id,
                    Status = sample.StageStatus,
                    StartedAt = stage.StartDate,
                    EndAt = stage.EndDate,
                    CompletedAt = sample.Status == ApplicationStatus.InProcess ? null : now,
                    CreatedAt = now
                });
                added++;
            }
        }

        await context.SaveChangesAsync(ct);
        Log.Information("Seeded {Count} sample applications across {BatchCount} active batches.",
            added, stagesByBatch.Count);
    }
}
