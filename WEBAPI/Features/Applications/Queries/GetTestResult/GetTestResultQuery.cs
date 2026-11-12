using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Queries.GetTestResult;

public sealed record GetTestResultQuery(Guid ApplicationStageId)
    : IRequest<Result<TestAttemptResultDto>>;
