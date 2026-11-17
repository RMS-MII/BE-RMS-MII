using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Queries.GetApplications;

public sealed record GetApplicationsQuery : IRequest<Result<PagedResult<BatchApplicationDto>>>
{
    public Guid BatchId { get; init; }
    public Guid? StageId { get; init; }
    public ApplicationStatus? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
