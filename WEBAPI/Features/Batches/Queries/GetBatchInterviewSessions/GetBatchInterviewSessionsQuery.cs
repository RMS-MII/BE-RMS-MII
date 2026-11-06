using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchInterviewSessions;

public sealed record GetBatchInterviewSessionsQuery : IRequest<Result<IReadOnlyCollection<InterviewSessionDto>>>
{
    public Guid BatchId { get; init; }
    public string? Search { get; init; }
    public Guid? StageId { get; init; }
    public bool? IsOnline { get; init; }
    public DateOnly? Date { get; init; }
}

