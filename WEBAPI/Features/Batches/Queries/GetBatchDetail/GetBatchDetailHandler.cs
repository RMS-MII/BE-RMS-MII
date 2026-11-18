using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDetail;

public sealed class GetBatchDetailHandler(RmsDbContext db)
    : IRequestHandler<GetBatchDetailQuery, Result<RecruitmentBatchDetailDto>>
{
    public async Task<Result<RecruitmentBatchDetailDto>> Handle(
        GetBatchDetailQuery request, CancellationToken ct)
    {
        var detail = await db.RecruitmentBatches
            .AsNoTracking()
            .Where(batch => batch.Id == request.Id)
            .Select(batch => new RecruitmentBatchDetailDto
            {
                Id = batch.Id,
                DepartmentId = batch.DepartmentId,
                DepartmentName = batch.Department.Name,
                BatchCode = batch.BatchCode,
                Name = batch.Name,
                Description = batch.Description,
                StartDate = batch.StartDate,
                EndDate = batch.EndDate,
                ApplicationDeadlineAt = batch.ApplicationDeadlineAt,
                Status = batch.Status,
                CreatedBy = batch.CreatedBy,
                CreatedAt = batch.CreatedAt,
                UpdatedAt = batch.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (detail is null)
            return Result<RecruitmentBatchDetailDto>.Failure(
                Error.NotFound("Batch was not found."));

        return Result<RecruitmentBatchDetailDto>.Success(detail);
    }
}
