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

        var descending = string.Equals(request.Order, "desc", StringComparison.OrdinalIgnoreCase);
        var sorted = request.SortBy?.ToLowerInvariant() switch
        {
            "code" => descending ? query.OrderByDescending(b => b.BatchCode) : query.OrderBy(b => b.BatchCode),
            "name" => descending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "stage" => descending
                ? query.OrderByDescending(b => b.BatchStages
                    .Where(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active)
                    .OrderBy(stage => stage.StartDate)
                    .Select(stage => stage.StageName).FirstOrDefault())
                : query.OrderBy(b => b.BatchStages
                    .Where(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Active)
                    .OrderBy(stage => stage.StartDate)
                    .Select(stage => stage.StageName).FirstOrDefault()),
            "candidates" => descending
                ? query.OrderByDescending(b => b.BatchApplications.Count(application => application.Status == ApplicationStatus.InProcess))
                : query.OrderBy(b => b.BatchApplications.Count(application => application.Status == ApplicationStatus.InProcess)),
            "period" => descending ? query.OrderByDescending(b => b.StartDate) : query.OrderBy(b => b.StartDate),
            "status" => descending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
            _ => query.OrderByDescending(b => b.CreatedAt)
        };

        var batches = await sorted
            .ThenBy(b => b.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var batchIds = batches.Select(batch => batch.Id).ToArray();
        var batchStages = await db.BatchStages
            .AsNoTracking()
            .Where(stage => batchIds.Contains(stage.RecruitmentBatchId))
            .Select(stage => new { stage.RecruitmentBatchId, stage.StageName, stage.LifecycleStatus })
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
            item.CanClose = item.Status == BatchStatus.Open && stages.Count > 0 &&
                stages.All(stage => stage.LifecycleStatus == BatchStageLifecycleStatus.Completed);
        }

        var result = new PagedResult<RecruitmentBatchDto>(
            items,
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<RecruitmentBatchDto>>.Success(result);
    }
}
