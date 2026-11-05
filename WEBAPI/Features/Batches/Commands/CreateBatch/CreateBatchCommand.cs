using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.CreateBatch;

public sealed record CreateBatchCommand : IRequest<Result<RecruitmentBatchDto>>
{
    public Guid DepartmentId { get; init; }
    public string BatchCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
