using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetAvailableInterviewersForStage;

public sealed record GetAvailableInterviewersForStageQuery(
    Guid ApplicationStageId) : IRequest<Result<IReadOnlyCollection<AssignedInterviewerDto>>>;

