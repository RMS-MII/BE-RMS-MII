using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.StageTypes.Commands.CreateStageType;

public sealed record CreateStageTypeCommand : IRequest<Result<StageTypeDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public StageCategory StageCategory { get; init; }
    public bool IsActive { get; init; } = true;
}

