using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerSessionDetail;

public sealed record GetInterviewerSessionDetailQuery(Guid SessionInterviewId)
    : IRequest<Result<InterviewerSessionDetailDto>>;
