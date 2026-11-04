using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;

public sealed record EvaluateStageCommand : IRequest<Result<ApplicationStageDto>>
{
    public Guid StageId { get; init; }
    public ApplicationStageStatus Status { get; init; }
    public string Notes { get; init; } = string.Empty;
}
