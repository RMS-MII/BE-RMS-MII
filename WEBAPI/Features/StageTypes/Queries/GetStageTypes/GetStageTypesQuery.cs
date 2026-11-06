using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.StageTypes.Queries.GetStageTypes;

public sealed record GetStageTypesQuery(
    StageCategory? Category = null,
    bool? ActiveOnly = null) : IRequest<Result<IReadOnlyCollection<StageTypeDto>>>;

