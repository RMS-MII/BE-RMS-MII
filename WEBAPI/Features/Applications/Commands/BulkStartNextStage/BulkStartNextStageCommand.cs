using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.BulkStartNextStage;

public sealed record BulkStartNextStageCommand(Guid BatchId, Guid StageId)
    : IRequest<Result<BulkStartNextStageDto>>;
