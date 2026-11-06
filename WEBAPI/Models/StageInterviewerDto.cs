using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

/// <summary>
/// Permintaan penugasan satu atau lebih karyawan sebagai pewawancara pada tahapan batch.
/// </summary>
public class AssignInterviewersRequest
{
    /// <summary>
    /// Daftar ID Karyawan yang ditugaskan sebagai pewawancara.
    /// </summary>
    public List<Guid>? EmployeeIds { get; set; }

    /// <summary>
    /// Alias dari EmployeeIds (keduanya didukung oleh backend).
    /// </summary>
    public List<Guid>? InterviewerEmployeeIds { get; set; }

    public List<Guid> GetEmployeeIds()
    {
        if (EmployeeIds is { Count: > 0 }) return EmployeeIds;
        if (InterviewerEmployeeIds is { Count: > 0 }) return InterviewerEmployeeIds;
        return [];
    }
}

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
