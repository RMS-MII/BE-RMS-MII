using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerSessions;

public sealed record GetInterviewerSessionsQuery(
    SessionInterviewStatus? Status = null,
    InterviewSessionStatus? SessionStatus = null,
    DateTime? From = null,
    DateTime? To = null,
    bool? PendingActionOnly = null) : IRequest<Result<IReadOnlyCollection<DashboardInterviewSessionDto>>>;

