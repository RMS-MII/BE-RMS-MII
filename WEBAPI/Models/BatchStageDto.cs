using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class BatchStageDto
{
    public Guid Id { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public Guid StageTypeId { get; set; }
    public string? StageTypeName { get; set; }
    public StageCategory? StageCategory { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal? PassingScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? NextStageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateBatchStageDto
{
    public Guid RecruitmentBatchId { get; init; }
    public Guid StageTypeId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public decimal? PassingScore { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? NextStageId { get; init; }
}

public record UpdateBatchStageDto
{
    public Guid StageTypeId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal? PassingScore { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? NextStageId { get; init; }
}
