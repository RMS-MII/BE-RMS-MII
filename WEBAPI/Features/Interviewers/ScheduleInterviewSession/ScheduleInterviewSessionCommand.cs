using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.ScheduleInterviewSession;

public record ScheduleInterviewSessionCommand(
    Guid ApplicationStageId,
    string SessionName,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    bool IsOnline,
    string? Location,
    string? MeetingUrl,
    List<Guid> InterviewerEmployeeIds,
    Guid? LeadEmployeeId = null) : IRequest<Result<InterviewSessionDto>>;
