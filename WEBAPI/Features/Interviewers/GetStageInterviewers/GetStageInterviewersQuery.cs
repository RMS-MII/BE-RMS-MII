using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetStageInterviewers;

public sealed record GetStageInterviewersQuery(
    Guid BatchId,
    Guid StageId,
    StageEmployeeStatus? Status = null) : IRequest<Result<AssignInterviewersResponse>>;

