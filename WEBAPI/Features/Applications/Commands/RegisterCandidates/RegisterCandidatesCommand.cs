using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.RegisterCandidates;

public sealed record RegisterCandidatesCommand : IRequest<Result<IReadOnlyCollection<BatchApplicationDto>>>
{
    public Guid BatchId { get; init; }
    public IReadOnlyList<CreateCandidateDto> Candidates { get; init; } = [];
}
