using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Templates;

namespace WEBAPI.Services;

public sealed class ApplicationStageProgressionService(
    RmsDbContext db,
    IConfiguration configuration)
{
    public async Task<IReadOnlyList<BatchStage>> GetBatchStagesAsync(
        Guid batchId,
        CancellationToken ct)
    {
        return await db.BatchStages
            .AsNoTracking()
            .Where(stage => stage.RecruitmentBatchId == batchId)
            .Include(stage => stage.StageType)
            .ToListAsync(ct);
    }

    public BatchStage? GetNextActiveStage(
        BatchStage currentStage,
        IReadOnlyDictionary<Guid, BatchStage> stagesById)
    {
        var nextStageId = currentStage.NextStageId;

        while (nextStageId.HasValue)
        {
            if (!stagesById.TryGetValue(nextStageId.Value, out var nextStage))
                return null;

            if (nextStage.IsActive)
                return nextStage;

            nextStageId = nextStage.NextStageId;
        }

        return null;
    }

    public async Task<TimeSpan?> GetDurationTestAsync(
        Guid departmentId,
        BatchStage nextStage,
        CancellationToken ct)
    {
        if (nextStage.StageType?.StageCategory != StageCategory.Test)
            return null;

        var durationInMinutes = await db.QuestionBanks
            .AsNoTracking()
            .Where(questionBank =>
                questionBank.DepartmentId == departmentId &&
                questionBank.StageTypeId == nextStage.StageTypeId)
            .OrderBy(questionBank => questionBank.CreatedAt)
            .ThenBy(questionBank => questionBank.Id)
            .Select(questionBank => (int?)questionBank.DurationInMinutes)
            .FirstOrDefaultAsync(ct);

        return durationInMinutes.HasValue
            ? TimeSpan.FromMinutes(durationInMinutes.Value)
            : null;
    }

    public ApplicationStage CreateApplicationStage(
        BatchApplication application,
        BatchStage nextStage,
        DateTime now,
        TimeSpan? durationTest)
    {
        application.UpdatedAt = now;

        return new ApplicationStage
        {
            Id = Guid.CreateVersion7(),
            BatchApplicationId = application.Id,
            BatchStageId = nextStage.Id,
            Status = ApplicationStageStatus.InProgress,
            StartedAt = now,
            EndAt = nextStage.EndDate,
            CompletedAt = null,
            Notes = null,
            IsAttempts = false,
            DurationTest = durationTest,
            CreatedAt = now
        };
    }

    public bool RequiresStageInvitation(BatchStage nextStage) =>
        nextStage.StageType?.StageCategory == StageCategory.Test;

    public EmailContent BuildStageInvitation(
        BatchApplication application,
        BatchStage batchStage,
        ApplicationStage applicationStage)
    {
        var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);
        var portalUrl = $"{frontendUrl}/portal/{application.CandidateId}";

        return EmailTemplates.StageInvitation(
            application.Candidate.FullName,
            application.RecruitmentBatch.Name,
            batchStage.StageName,
            portalUrl,
            applicationStage.EndAt);
    }
}
