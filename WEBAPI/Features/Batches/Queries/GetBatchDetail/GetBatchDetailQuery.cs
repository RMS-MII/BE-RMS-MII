using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDetail;

public sealed record GetBatchDetailQuery(Guid Id) : IRequest<Result<RecruitmentBatchDetailDto>>;
