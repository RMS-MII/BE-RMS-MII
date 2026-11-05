using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.ConfirmSchedule;

public sealed record ConfirmScheduleCommand(
    Guid CandidateId,
    Guid SessionId) : IRequest<Result<ConfirmScheduleResultDto>>;
