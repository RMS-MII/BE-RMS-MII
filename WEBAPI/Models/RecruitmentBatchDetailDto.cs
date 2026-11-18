using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class RecruitmentBatchDetailDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ApplicationDeadlineAt { get; set; }
    public BatchStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
