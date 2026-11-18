using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchStageFlow;

public sealed record GetBatchStageFlowQuery(Guid BatchId)
    : IRequest<Result<IReadOnlyCollection<BatchStageFlowDto>>>;
