using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.RescheduleRequests.Queries.GetRescheduleRequests;

public record GetRescheduleRequestsQuery(
    RescheduleStatus? Status = null,
    Guid? SessionId = null) : IRequest<Result<IReadOnlyCollection<RescheduleRequestDto>>>;

