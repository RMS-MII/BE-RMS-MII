using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchStages;

public sealed record GetBatchStagesQuery(Guid BatchId)
    : IRequest<Result<IReadOnlyCollection<BatchStageWithInterviewersDto>>>;
