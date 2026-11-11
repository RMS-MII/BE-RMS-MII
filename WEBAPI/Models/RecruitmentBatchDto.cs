using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class RecruitmentBatchDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BatchStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<BatchStageDto> BatchStages { get; set; } = [];
    public string? CurrentStageName { get; set; }
    public int? ActiveCandidateCount { get; set; }
    public bool? CanClose { get; set; }
}

public record CreateRecruitmentBatchDto
{
    public Guid DepartmentId { get; init; }
    public string BatchCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record UpdateRecruitmentBatchDto
{
    public Guid DepartmentId { get; init; }
    public string BatchCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public BatchStatus Status { get; init; }
}
