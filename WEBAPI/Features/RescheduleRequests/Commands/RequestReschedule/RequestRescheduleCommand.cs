using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;

public record RequestRescheduleCommand(
    Guid InterviewSessionId,
    DateTime ProposedStart,
    DateTime ProposedEnd,
    string Reason,
    Guid? CandidateId = null) : IRequest<Result<RescheduleRequestDto>>;

