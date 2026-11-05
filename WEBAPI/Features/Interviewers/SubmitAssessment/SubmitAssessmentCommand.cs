using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.SubmitAssessment;

public record SubmitAssessmentCommand(
    Guid SessionInterviewId,
    decimal Score,
    Recommendation Recommendation,
    string? Notes) : IRequest<Result<AssessmentDto>>;
