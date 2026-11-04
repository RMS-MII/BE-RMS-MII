using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerDashboard;

public record GetInterviewerDashboardQuery : IRequest<Result<InterviewerDashboardDto>>;
