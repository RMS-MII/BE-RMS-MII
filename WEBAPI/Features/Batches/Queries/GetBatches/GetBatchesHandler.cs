using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatches;

public sealed class GetBatchesHandler(RmsDbContext db)
    : IRequestHandler<GetBatchesQuery, Result<PagedResult<RecruitmentBatchDto>>>
{
    public async Task<Result<PagedResult<RecruitmentBatchDto>>> Handle(
        GetBatchesQuery request, CancellationToken ct)
    {
        var query = db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(b => b.DepartmentId == request.DepartmentId.Value);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.BatchCode.Contains(search) || b.Name.Contains(search));

        var totalItems = await query.CountAsync(ct);

        var batches = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var batchIds = batches.Select(batch => batch.Id).ToArray();
        var batchStages = await db.BatchStages
            .AsNoTracking()
            .Where(stage => batchIds.Contains(stage.RecruitmentBatchId))
            .Select(stage => new { stage.RecruitmentBatchId, stage.StageName, stage.IsActive, stage.LifecycleStatus })
            .ToListAsync(ct);
        var stagesByBatch = batchStages
            .GroupBy(stage => stage.RecruitmentBatchId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var activeCandidateCounts = await db.BatchApplications
            .AsNoTracking()
            .Where(application => batchIds.Contains(application.RecruitmentBatchId)
                && application.Status == ApplicationStatus.InProcess)
            .GroupBy(application => application.RecruitmentBatchId)
            .Select(group => new { BatchId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.BatchId, item => item.Count, ct);

        var items = batches.ToDtoList();
        foreach (var item in items)
        {
            var stages = stagesByBatch.GetValueOrDefault(item.Id) ?? [];
            item.CurrentStageName = stages.FirstOrDefault(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active)?.StageName;
            item.ActiveCandidateCount = activeCandidateCounts.GetValueOrDefault(item.Id);
            var configuredStages = stages.Where(stage => stage.IsActive).ToList();
            item.CanClose = item.Status == BatchStatus.Open && configuredStages.Count > 0 &&
                configuredStages.All(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Completed);
        }

        var result = new PagedResult<RecruitmentBatchDto>(
            items,
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<RecruitmentBatchDto>>.Success(result);
    }
}
