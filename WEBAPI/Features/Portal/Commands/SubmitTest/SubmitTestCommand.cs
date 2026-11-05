using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.SubmitTest;

public sealed record SubmitTestCommand : IRequest<Result<SubmitTestResultDto>>
{
    public Guid CandidateId { get; init; }
    public Guid StageId { get; init; }
    public IReadOnlyList<SubmitTestAnswerItem> Answers { get; init; } = [];
}

public sealed record SubmitTestAnswerItem
{
    public Guid QuestionId { get; init; }
    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = [];
}
