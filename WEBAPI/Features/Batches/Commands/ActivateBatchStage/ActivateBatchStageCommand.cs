using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.ActivateBatchStage;

public sealed record ActivateBatchStageCommand(Guid BatchId, Guid StageId)
    : IRequest<Result<BatchStageTransitionDto>>;
