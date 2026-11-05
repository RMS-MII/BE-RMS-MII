using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Queries.GetTest;

public sealed record GetTestQuery(Guid CandidateId, Guid StageId)
    : IRequest<Result<CandidateTestDto>>;
