using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.UpdateBatchStatus;

public sealed record UpdateBatchStatusCommand : IRequest<Result<RecruitmentBatchDto>>
{
    public Guid BatchId { get; init; }
    public BatchStatus Status { get; init; }
}

public sealed record UpdateBatchStatusRequest
{
    /// <summary>
    /// Status tujuan: Open untuk mengaktifkan batch Draft, atau Closed untuk menutup batch Open.
    /// </summary>
    public BatchStatus Status { get; init; }
}
