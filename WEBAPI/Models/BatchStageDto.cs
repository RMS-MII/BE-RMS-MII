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
    public BatchStageLifecycleStatus LifecycleStatus { get; set; }
    public int? CandidateCount { get; set; }
    public decimal? PassingScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? NextStageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BatchStageWithInterviewersDto : BatchStageDto
{
    public List<AssignedInterviewerDto> Interviewers { get; set; } = [];
}

public sealed class BatchStageTransitionDto
{
    public Guid BatchStageId { get; set; }
    public BatchStageLifecycleStatus LifecycleStatus { get; set; }
    public int CandidatesStarted { get; set; }
    public int CandidatesPassed { get; set; }
    public int CandidatesRejected { get; set; }
    public int CandidatesDropped { get; set; }
    public int CandidatesExpired { get; set; }
    public bool AlreadyApplied { get; set; }
}

public record CreateBatchStageDto
{
    public Guid RecruitmentBatchId { get; init; }
    public Guid StageTypeId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public decimal? PassingScore { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? NextStageId { get; init; }
}

public record UpdateBatchStageDto
{
    public Guid StageTypeId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public decimal? PassingScore { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? NextStageId { get; init; }
}
