using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Batches.Commands.ConfigureBatchStages;

public sealed class ConfigureBatchStagesHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<ConfigureBatchStagesHandler> logger)
    : IRequestHandler<ConfigureBatchStagesCommand, Result<IReadOnlyCollection<BatchStageDto>>>
{
    public async Task<Result<IReadOnlyCollection<BatchStageDto>>> Handle(
        ConfigureBatchStagesCommand request, CancellationToken ct)
    {
        var batchExists = await db.RecruitmentBatches
            .AnyAsync(b => b.Id == request.BatchId, ct);

        if (!batchExists)
            return Result<IReadOnlyCollection<BatchStageDto>>.Failure(
                Error.NotFound("Batch was not found."));

        var isInUse = await db.BatchStages
            .AnyAsync(s => s.RecruitmentBatchId == request.BatchId &&
                           (s.ApplicationStages.Any() ||
                            s.StageEmployees.Any() ||
                            s.LifecycleStatus != WEBAPI.Entities.Enums.BatchStageLifecycleStatus.Inactive), ct);

        if (isInUse)
            return Result<IReadOnlyCollection<BatchStageDto>>.Failure(
                Error.BusinessRule(
                    "Stages cannot be reconfigured after candidates or interviewers are assigned, or a stage has been activated."));

        var requestedTypeIds = request.Stages
            .Select(stage => stage.StageTypeId)
            .Distinct()
            .ToList();

        var stageTypes = await db.StageTypes
            .Where(stageType => requestedTypeIds.Contains(stageType.Id))
            .ToDictionaryAsync(stageType => stageType.Id, ct);

        var unknownTypeIds = requestedTypeIds
            .Where(id => !stageTypes.ContainsKey(id))
            .ToList();

        if (unknownTypeIds.Count > 0)
            return Result<IReadOnlyCollection<BatchStageDto>>.Failure(
                Error.Validation($"Unknown stage type id(s): {string.Join(", ", unknownTypeIds)}."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            var existingStages = await db.BatchStages
                .Where(stage => stage.RecruitmentBatchId == request.BatchId)
                .ToListAsync(ct);

            if (existingStages.Count > 0)
            {
                // Putuskan rantai dulu agar penghapusan tidak melanggar self-referencing FK.
                foreach (var stage in existingStages)
                    stage.NextStageId = null;

                await db.SaveChangesAsync(ct);

                db.BatchStages.RemoveRange(existingStages);
                await db.SaveChangesAsync(ct);
            }

            var newStages = request.Stages
                .Select(item => new BatchStage
                {
                    Id = Guid.NewGuid(),
                    RecruitmentBatchId = request.BatchId,
                    StageTypeId = item.StageTypeId,
                    StageType = stageTypes[item.StageTypeId],
                    StageName = item.StageName?.Trim() ?? string.Empty,
                    IsActive = item.IsActive,
                    LifecycleStatus = WEBAPI.Entities.Enums.BatchStageLifecycleStatus.Inactive,
                    PassingScore = item.PassingScore,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    CreatedAt = now
                })
                .ToList();

            db.BatchStages.AddRange(newStages);
            await db.SaveChangesAsync(ct);

            // Link dibuat setelah semua baris tersimpan supaya FK NextStageId tidak menunjuk baris yang belum ada.
            for (var i = 0; i < newStages.Count - 1; i++)
                newStages[i].NextStageId = newStages[i + 1].Id;

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result<IReadOnlyCollection<BatchStageDto>>.Success(newStages.ToDtoList());
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            logger.LogWarning(
                "Configure stages gagal: nama stage duplikat pada batch {BatchId}", request.BatchId);

            return Result<IReadOnlyCollection<BatchStageDto>>.Failure(
                Error.Conflict("Stage names must be unique within a batch."));
        }
    }
}
