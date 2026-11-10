using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class SessionInterviewMapping
{
    public static ScheduledInterviewerDto ToScheduledInterviewerDto(this SessionInterview si) => new()
    {
        SessionInterviewId = si.Id,
        StageEmployeeId = si.StageEmployeeId,
        EmployeeId = si.StageEmployee?.EmployeeId ?? Guid.Empty,
        FullName = si.StageEmployee?.Employee?.FullName ?? string.Empty,
        Email = si.StageEmployee?.Employee?.Email ?? string.Empty,
        JobTitle = si.StageEmployee?.Employee?.JobTitle ?? string.Empty,
        DepartmentName = si.StageEmployee?.Employee?.Department?.Name ?? string.Empty,
        Status = si.Status,
        IsLead = si.IsLead
    };

    public static List<ScheduledInterviewerDto> ToScheduledInterviewerDtoList(
        this IEnumerable<SessionInterview> sessionInterviews)
        => sessionInterviews.Select(ToScheduledInterviewerDto).ToList();

    public static ConfirmSessionResponse ToConfirmSessionResponse(
        this SessionInterview sessionInterview,
        bool emailSent,
        string message)
    {
        var session = sessionInterview.InterviewSession;
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;

        return new ConfirmSessionResponse
        {
            SessionInterviewId = sessionInterview.Id,
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            IsOnline = session.IsOnline,
            MeetingUrl = session.MeetingUrl,
            Location = session.Location,
            SessionStatus = session.Status,
            CandidateName = candidate?.FullName ?? string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            CandidateInvitationEmailSent = emailSent,
            Message = message
        };
    }
}

