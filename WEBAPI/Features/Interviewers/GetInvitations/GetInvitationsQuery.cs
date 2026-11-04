using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInvitations;

public record GetInvitationsQuery(StageEmployeeStatus? Status = null) : IRequest<Result<List<InterviewerInvitationDto>>>;
