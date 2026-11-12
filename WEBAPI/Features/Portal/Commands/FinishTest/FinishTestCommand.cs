using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.FinishTest;

public sealed record FinishTestCommand(Guid CandidateId, Guid StageId)
    : IRequest<Result<FinishTestResultDto>>;
