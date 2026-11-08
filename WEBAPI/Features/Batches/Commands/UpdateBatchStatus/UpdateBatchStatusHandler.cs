using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Batches.Commands.UpdateBatchStatus;

public sealed class UpdateBatchStatusHandler(RmsDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateBatchStatusCommand, Result<RecruitmentBatchDto>>
{
    public async Task<Result<RecruitmentBatchDto>> Handle(
        UpdateBatchStatusCommand request, CancellationToken ct)
    {
        var batch = await db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .Include(b => b.BatchStages)
                .ThenInclude(s => s.StageType)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, ct);

        if (batch is null)
            return Result<RecruitmentBatchDto>.Failure(Error.NotFound("Batch was not found."));

        if (!batch.Status.CanTransitionTo(request.Status))
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                $"Cannot change recruitment batch status from {batch.Status} to {request.Status}. " +
                "Allowed transitions are Draft to Open and Open to Closed."));

        if (request.Status == BatchStatus.Open && batch.BatchStages.Count == 0)
            return Result<RecruitmentBatchDto>.Failure(Error.BusinessRule(
                "Configure at least one selection stage before opening this recruitment batch."));

        if (batch.Status != request.Status)
        {
            var now = dateTimeProvider.GetUtcNow().UtcDateTime;

            // Recheck the current status and stage requirement in the update itself.
            var updated = await db.RecruitmentBatches
                .Where(b => b.Id == request.BatchId && b.Status == batch.Status)
                .Where(b => request.Status != BatchStatus.Open || b.BatchStages.Any())
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Status, request.Status)
                    .SetProperty(b => b.UpdatedAt, now), ct);

            if (updated == 0)
                return Result<RecruitmentBatchDto>.Failure(Error.Conflict(
                    "The recruitment batch changed while updating its status. Refresh the batch and try again."));

            batch.Status = request.Status;
            batch.UpdatedAt = now;
        }

        var dto = batch.ToDto();
        dto.BatchStages = dto.BatchStages.OrderBy(s => s.StartDate).ThenBy(s => s.CreatedAt).ToList();
        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
