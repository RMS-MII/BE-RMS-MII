using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDetail;

public sealed class GetBatchDetailHandler(RmsDbContext db)
    : IRequestHandler<GetBatchDetailQuery, Result<RecruitmentBatchDto>>
{
    public async Task<Result<RecruitmentBatchDto>> Handle(
        GetBatchDetailQuery request, CancellationToken ct)
    {
        var batch = await db.RecruitmentBatches
            .AsNoTracking()
            .Include(b => b.Department)
            .Include(b => b.BatchStages)
                .ThenInclude(s => s.StageType)
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (batch is null)
            return Result<RecruitmentBatchDto>.Failure(
                Error.NotFound("Batch was not found."));

        var stageIds = batch.BatchStages.Select(stage => stage.Id).ToArray();
        var candidateCounts = await db.ApplicationStages
            .AsNoTracking()
            .Where(stage => stageIds.Contains(stage.BatchStageId))
            .GroupBy(stage => stage.BatchStageId)
            .Select(group => new { StageId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.StageId, item => item.Count, ct);

        var dto = batch.ToDto();
        dto.BatchStages = dto.BatchStages
            .Select(stage =>
            {
                stage.CandidateCount = candidateCounts.GetValueOrDefault(stage.Id);
                return stage;
            })
            .ToList();

        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
