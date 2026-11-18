using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchSummary;

public sealed record GetBatchSummaryQuery(Guid BatchId) : IRequest<Result<BatchDetailSummaryDto>>;
