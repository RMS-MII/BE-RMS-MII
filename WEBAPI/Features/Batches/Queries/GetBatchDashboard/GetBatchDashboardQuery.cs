using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDashboard;

public sealed record GetBatchDashboardQuery : IRequest<Result<GetBatchDashboardDto>>;
