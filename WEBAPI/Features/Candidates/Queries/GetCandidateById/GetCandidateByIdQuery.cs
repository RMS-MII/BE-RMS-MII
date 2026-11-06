using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Candidates.Queries.GetCandidateById;

public sealed record GetCandidateByIdQuery(Guid Id) : IRequest<Result<CandidateWithBatchesDto>>;

