using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.SetFinalResult;

public sealed record SetFinalResultCommand : IRequest<Result<BatchApplicationDto>>
{
    public Guid ApplicationId { get; init; }
    public FinalResultStatus FinalResultStatus { get; init; }
    public string Notes { get; init; } = string.Empty;
}
