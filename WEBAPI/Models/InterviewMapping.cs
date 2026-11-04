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
}
