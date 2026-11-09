using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.CompleteBatchStage;

public sealed record CompleteBatchStageCommand(Guid BatchId, Guid StageId)
    : IRequest<Result<BatchStageTransitionDto>>;
