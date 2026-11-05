using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.UploadSubmission;

public record UploadSubmissionCommand(
    Guid CandidateId,
    Guid InterviewSessionId,
    string FileSubmission) : IRequest<Result<SubmissionDto>>;

public record UploadSubmissionRequest(string FileSubmission);

