using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class StageInterviewerMapping
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
}

