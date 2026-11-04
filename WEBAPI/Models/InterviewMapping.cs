using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class InterviewMapping
{
    public static AssignedInterviewerDto ToAssignedInterviewerDto(this StageEmployee se) => new()
    {
        StageEmployeeId = se.Id,
        EmployeeId = se.EmployeeId,
        FullName = se.Employee?.FullName ?? string.Empty,
        Email = se.Employee?.Email ?? string.Empty,
        JobTitle = se.Employee?.JobTitle ?? string.Empty,
        DepartmentName = se.Employee?.Department?.Name ?? string.Empty,
        Status = se.Status
    };

    public static List<AssignedInterviewerDto> ToAssignedInterviewerDtoList(
        this IEnumerable<StageEmployee> stageEmployees)
        => stageEmployees.Select(ToAssignedInterviewerDto).ToList();

    public static AssignInterviewersResponse ToAssignInterviewersResponse(
        this BatchStage stage,
        IEnumerable<AssignedInterviewerDto> interviewers) => new()
    {
        BatchId = stage.RecruitmentBatchId,
        BatchName = stage.RecruitmentBatch?.Name ?? string.Empty,
        StageId = stage.Id,
        StageName = stage.StageName,
        Interviewers = interviewers.ToList()
    };

    public static InterviewerInvitationDto ToInterviewerInvitationDto(this StageEmployee se) => new()
    {
        Id = se.Id,
        BatchId = se.BatchStage?.RecruitmentBatchId ?? Guid.Empty,
        BatchCode = se.BatchStage?.RecruitmentBatch?.BatchCode ?? string.Empty,
        BatchName = se.BatchStage?.RecruitmentBatch?.Name ?? string.Empty,
        StageId = se.BatchStageId,
        StageName = se.BatchStage?.StageName ?? string.Empty,
        StageTypeName = se.BatchStage?.StageType?.Name ?? string.Empty,
        StageStartDate = se.BatchStage?.StartDate ?? DateTime.MinValue,
        StageEndDate = se.BatchStage?.EndDate ?? DateTime.MinValue,
        Status = se.Status,
        Note = se.Note,
        AssignedAt = se.CreatedAt,
        RespondedAt = se.UpdatedAt
    };

    public static List<InterviewerInvitationDto> ToInterviewerInvitationDtoList(
        this IEnumerable<StageEmployee> stageEmployees)
        => stageEmployees.Select(ToInterviewerInvitationDto).ToList();

    public static RespondInvitationResponse ToRespondInvitationResponse(
        this StageEmployee se,
        DateTime respondedAt) => new()
    {
        InvitationId = se.Id,
        BatchId = se.BatchStage?.RecruitmentBatchId ?? Guid.Empty,
        BatchName = se.BatchStage?.RecruitmentBatch?.Name ?? string.Empty,
        StageId = se.BatchStageId,
        StageName = se.BatchStage?.StageName ?? string.Empty,
        Status = se.Status,
        Note = se.Note,
        RespondedAt = respondedAt
    };

    public static ScheduledInterviewerDto ToScheduledInterviewerDto(this SessionInterview si) => new()
    {
        SessionInterviewId = si.Id,
        StageEmployeeId = si.StageEmployeeId,
        EmployeeId = si.StageEmployee?.EmployeeId ?? Guid.Empty,
        FullName = si.StageEmployee?.Employee?.FullName ?? string.Empty,
        Email = si.StageEmployee?.Employee?.Email ?? string.Empty,
        JobTitle = si.StageEmployee?.Employee?.JobTitle ?? string.Empty,
        DepartmentName = si.StageEmployee?.Employee?.Department?.Name ?? string.Empty,
        Status = si.Status
    };

    public static List<ScheduledInterviewerDto> ToScheduledInterviewerDtoList(
        this IEnumerable<SessionInterview> sessionInterviews)
        => sessionInterviews.Select(ToScheduledInterviewerDto).ToList();

    public static InterviewSessionDto ToInterviewSessionDto(this InterviewSession session) => new()
    {
        Id = session.Id,
        ApplicationStageId = session.ApplicationStageId,
        CandidateName = session.ApplicationStage?.BatchApplication?.Candidate?.FullName ?? string.Empty,
        CandidateEmail = session.ApplicationStage?.BatchApplication?.Candidate?.Email ?? string.Empty,
        BatchName = session.ApplicationStage?.BatchStage?.RecruitmentBatch?.Name ?? string.Empty,
        StageName = session.ApplicationStage?.BatchStage?.StageName ?? string.Empty,
        SessionName = session.SessionName,
        ScheduledStart = session.ScheduledStart,
        ScheduledEnd = session.ScheduledEnd,
        IsOnline = session.IsOnline,
        Location = session.Location,
        MeetingUrl = session.MeetingUrl,
        Status = session.Status,
        Interviewers = session.SessionInterviews.ToScheduledInterviewerDtoList(),
        CreatedAt = session.CreatedAt
    };

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

    public static DashboardInterviewSessionDto ToDashboardInterviewSessionDto(this SessionInterview si)
    {
        var session = si.InterviewSession;
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;
        var stage = session.ApplicationStage?.BatchStage;
        var batch = stage?.RecruitmentBatch;
        var assessment = si.InterviewAssessment;

        return new DashboardInterviewSessionDto
        {
            SessionInterviewId = si.Id,
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            IsOnline = session.IsOnline,
            MeetingUrl = session.MeetingUrl,
            Location = session.Location,
            SessionStatus = session.Status,
            InterviewerStatus = si.Status,
            CandidateId = candidate?.Id ?? Guid.Empty,
            CandidateName = candidate?.FullName ?? string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            BatchName = batch?.Name ?? string.Empty,
            StageName = stage?.StageName ?? string.Empty,
            HasTakeHome = session.EndedSubmissionAt.HasValue || !string.IsNullOrWhiteSpace(session.FileSubmission),
            EndedSubmissionAt = session.EndedSubmissionAt,
            FileSubmission = session.FileSubmission,
            IsAssessmentSubmitted = assessment is not null,
            AssessmentScore = assessment?.Score,
            AssessmentRecommendation = assessment?.Recommendation
        };
    }
}
