using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
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
            query = query.Where(application =>
                application.ApplicationStages.Any(stage => stage.BatchStageId == request.StageId.Value));
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

        var applications = await query
            .Include(application => application.Candidate)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.BatchStage)
            .OrderByDescending(application => application.AppliedAt)
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
