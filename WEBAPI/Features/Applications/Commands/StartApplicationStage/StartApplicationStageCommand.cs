using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.StartApplicationStage;

public sealed record StartApplicationStageCommand(Guid ApplicationId)
    : IRequest<Result<ApplicationStageDto>>;
