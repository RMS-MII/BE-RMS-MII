using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetSubmission;

public record GetSubmissionQuery(Guid InterviewSessionId) : IRequest<Result<SubmissionDto>>;
