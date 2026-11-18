using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetInterviewCandidates;

public sealed record GetInterviewCandidatesQuery(Guid BatchId, string? Search, int Page = 1)
    : IRequest<Result<InterviewCandidatesPageDto>>;
