using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.RespondInvitation;

public record RespondInvitationCommand(
    Guid InvitationId,
    StageEmployeeStatus Response,
    string? Note = null) : IRequest<Result<RespondInvitationResponse>>;
