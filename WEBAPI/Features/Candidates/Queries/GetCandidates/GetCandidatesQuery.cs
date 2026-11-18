using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Candidates.Queries.GetCandidates;

public sealed record GetCandidatesQuery : IRequest<Result<PagedResult<CandidateWithBatchesDto>>>
{
    public string? Search { get; init; }
    public Guid? BatchId { get; init; }
    public Gender? Gender { get; init; }
    public string? BatchStatus { get; init; }
    public string? SortBy { get; init; }
    public string? Order { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

