using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.StageTypes.Queries.GetStageTypeById;

public sealed record GetStageTypeByIdQuery(Guid Id) : IRequest<Result<StageTypeDto>>;

