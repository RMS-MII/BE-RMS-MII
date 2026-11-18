using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetAvailableTalentCandidates;

public sealed record GetAvailableTalentCandidatesQuery(Guid BatchId, string? Search, int Page = 1, int PageSize = 5)
    : IRequest<Result<AvailableTalentCandidatesPageDto>>;
