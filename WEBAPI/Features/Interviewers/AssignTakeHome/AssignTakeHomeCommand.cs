using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.AssignTakeHome;

public record AssignTakeHomeCommand(
    Guid InterviewSessionId,
    DateTime EndedSubmissionAt,
    string TaskTitle,
    string Instructions,
    string FileTakeHome) : IRequest<Result<AssignTakeHomeResponse>>;
