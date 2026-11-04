using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public record AssignInterviewersRequest(List<Guid> InterviewerEmployeeIds);

public class AssignedInterviewerDto
{
    public Guid StageEmployeeId { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
}

public class AssignInterviewersResponse
{
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public List<AssignedInterviewerDto> Interviewers { get; set; } = [];
}
