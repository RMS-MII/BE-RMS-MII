using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;

public record ReviewRescheduleCommand(
    Guid RescheduleRequestId,
    RescheduleStatus Decision,
    string? ReviewNotes) : IRequest<Result<RescheduleRequestDto>>;

