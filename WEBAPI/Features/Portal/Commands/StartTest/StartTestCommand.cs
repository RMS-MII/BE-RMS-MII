using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.StartTest;

public sealed record StartTestCommand(Guid CandidateId, Guid StageId)
    : IRequest<Result<StartTestResultDto>>;
