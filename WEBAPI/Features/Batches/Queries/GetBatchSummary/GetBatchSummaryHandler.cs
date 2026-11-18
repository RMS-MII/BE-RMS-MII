using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchSummary;

public sealed class GetBatchSummaryHandler(RmsDbContext db)
    : IRequestHandler<GetBatchSummaryQuery, Result<BatchDetailSummaryDto>>
{
    public async Task<Result<BatchDetailSummaryDto>> Handle(
        GetBatchSummaryQuery request, CancellationToken ct)
    {
        var owner = await db.RecruitmentBatches
            .AsNoTracking()
            .Where(batch => batch.Id == request.BatchId)
            .Select(batch => new
            {
                OwnerName = db.Employees
                    .Where(employee => employee.Id == batch.CreatedBy)
                    .Select(employee => employee.FullName)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (owner is null)
            return Result<BatchDetailSummaryDto>.Failure(
                Error.NotFound("Batch was not found."));

        var counts = await db.BatchApplications
            .AsNoTracking()
            .Where(application => application.RecruitmentBatchId == request.BatchId)
            .GroupBy(application => application.RecruitmentBatchId)
            .Select(group => new
            {
                Total = group.Count(),
                Active = group.Count(application => application.Status == ApplicationStatus.InProcess),
                Rejected = group.Count(application => application.Status == ApplicationStatus.Rejected),
                Dropped = group.Count(application => application.Status == ApplicationStatus.Dropped)
            })
            .FirstOrDefaultAsync(ct);

        var interviewSessions = await db.InterviewSessions.AsNoTracking()
            .CountAsync(session => session.ApplicationStage.BatchApplication.RecruitmentBatchId == request.BatchId, ct);

        return Result<BatchDetailSummaryDto>.Success(new BatchDetailSummaryDto
        {
            OwnerName = owner.OwnerName,
            TotalCandidates = counts?.Total ?? 0,
            ActiveCandidates = counts?.Active ?? 0,
            RejectedCandidates = counts?.Rejected ?? 0,
            DroppedCandidates = counts?.Dropped ?? 0,
            InterviewSessions = interviewSessions
        });
    }
}
