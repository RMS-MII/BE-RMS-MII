using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.AssignInterviewers;

public record AssignInterviewersCommand(
    Guid BatchId,
    Guid StageId,
    List<Guid> InterviewerEmployeeIds) : IRequest<Result<AssignInterviewersResponse>>;
