using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Queries.GetCandidateTasks;

public sealed record GetCandidateTasksQuery(Guid CandidateId)
    : IRequest<Result<CandidateTasksDto>>;
