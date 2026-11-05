using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatches;

public sealed record GetBatchesQuery : IRequest<Result<PagedResult<RecruitmentBatchDto>>>
{
    public BatchStatus? Status { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
