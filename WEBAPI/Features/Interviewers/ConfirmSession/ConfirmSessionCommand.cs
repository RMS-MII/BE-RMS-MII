using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.ConfirmSession;

public record ConfirmSessionCommand(Guid SessionInterviewId) : IRequest<Result<ConfirmSessionResponse>>;
