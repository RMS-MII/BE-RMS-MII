using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Features.Batches;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Queries.GetApplications;

public sealed class GetApplicationsHandler(RmsDbContext db)
    : IRequestHandler<GetApplicationsQuery, Result<PagedResult<BatchApplicationDto>>>
{
    public async Task<Result<PagedResult<BatchApplicationDto>>> Handle(
        GetApplicationsQuery request, CancellationToken ct)
    {
        var batchExists = await db.RecruitmentBatches
            .AsNoTracking()
            .AnyAsync(batch => batch.Id == request.BatchId, ct);

        if (!batchExists)
            return Result<PagedResult<BatchApplicationDto>>.Failure(
                Error.NotFound("Batch was not found."));

        var query = db.BatchApplications
            .AsNoTracking()
            .Where(application => application.RecruitmentBatchId == request.BatchId);

        if (request.StageId.HasValue)
        {
            var stageId = request.StageId.Value;
            var hasUnstartedStandalone = await db.BatchApplications
                .AsNoTracking()
                .AnyAsync(application => application.RecruitmentBatchId == request.BatchId &&
                                         application.IsStandalone &&
                                         !application.ApplicationStages.Any(), ct);

            Guid? firstStageId = null;
            if (hasUnstartedStandalone)
            {
                var batchStages = await db.BatchStages
                    .AsNoTracking()
                    .Where(stage => stage.RecruitmentBatchId == request.BatchId)
                    .ToListAsync(ct);
                firstStageId = BatchStageSequence.GetOrderedStages(batchStages).FirstOrDefault()?.Id;
            }

            // Kandidat standalone yang baru didaftarkan belum punya ApplicationStage sampai HR menekan Start.
            // Tampilkan mereka di filter stage pertama agar tidak hilang dari daftar, tanpa menjalankan stage lebih awal.
            query = query.Where(application =>
                application.ApplicationStages.Any(stage => stage.BatchStageId == stageId) ||
                (firstStageId == stageId && application.IsStandalone &&
                 !application.ApplicationStages.Any()));
        }

        if (request.Status.HasValue)
            query = query.Where(application => application.Status == request.Status.Value);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(application =>
                application.CodeApplication.Contains(search) ||
                application.Candidate.FullName.Contains(search) ||
                application.Candidate.Email.Contains(search));
        }

        var totalItems = await query.CountAsync(ct);

        var descending = string.Equals(request.Order, "desc", StringComparison.OrdinalIgnoreCase);
        var sorted = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(application => application.Candidate.FullName)
                : query.OrderBy(application => application.Candidate.FullName),
            "code" => descending ? query.OrderByDescending(application => application.CodeApplication)
                : query.OrderBy(application => application.CodeApplication),
            "stage" => descending
                ? query.OrderByDescending(application => application.ApplicationStages
                    .Where(stage => !request.StageId.HasValue || stage.BatchStageId == request.StageId.Value)
                    .OrderByDescending(stage => stage.BatchStage.StartDate)
                    .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                    .ThenByDescending(stage => stage.StartedAt)
                    .Select(stage => stage.BatchStage.StageName).FirstOrDefault())
                : query.OrderBy(application => application.ApplicationStages
                    .Where(stage => !request.StageId.HasValue || stage.BatchStageId == request.StageId.Value)
                    .OrderByDescending(stage => stage.BatchStage.StartDate)
                    .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                    .ThenByDescending(stage => stage.StartedAt)
                    .Select(stage => stage.BatchStage.StageName).FirstOrDefault()),
            "status" when !request.StageId.HasValue => descending
                ? query.OrderByDescending(application => application.Status)
                    .ThenByDescending(application => application.ApplicationStages
                        .OrderByDescending(stage => stage.BatchStage.StartDate)
                        .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                        .ThenByDescending(stage => stage.StartedAt)
                        .Select(stage => (int?)stage.Status).FirstOrDefault())
                : query.OrderBy(application => application.Status)
                    .ThenBy(application => application.ApplicationStages
                        .OrderByDescending(stage => stage.BatchStage.StartDate)
                        .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                        .ThenByDescending(stage => stage.StartedAt)
                        .Select(stage => (int?)stage.Status).FirstOrDefault()),
            "status" => descending
                ? query.OrderByDescending(application => application.ApplicationStages
                    .Where(stage => !request.StageId.HasValue || stage.BatchStageId == request.StageId.Value)
                    .OrderByDescending(stage => stage.BatchStage.StartDate)
                    .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                    .ThenByDescending(stage => stage.StartedAt)
                    .Select(stage => (int?)stage.Status).FirstOrDefault())
                : query.OrderBy(application => application.ApplicationStages
                    .Where(stage => !request.StageId.HasValue || stage.BatchStageId == request.StageId.Value)
                    .OrderByDescending(stage => stage.BatchStage.StartDate)
                    .ThenByDescending(stage => stage.BatchStage.CreatedAt)
                    .ThenByDescending(stage => stage.StartedAt)
                    .Select(stage => (int?)stage.Status).FirstOrDefault()),
            "appliedat" => descending ? query.OrderByDescending(application => application.AppliedAt)
                : query.OrderBy(application => application.AppliedAt),
            _ => query.OrderByDescending(application => application.AppliedAt)
        };

        var applications = await sorted
            .ThenBy(application => application.Id)
            .Include(application => application.Candidate)
            .Include(application => application.RecruitmentBatch)
                .ThenInclude(batch => batch.BatchStages)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.BatchStage)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var result = new PagedResult<BatchApplicationDto>(
            applications.ToDtoList(request.StageId),
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<BatchApplicationDto>>.Success(result);
    }
}
