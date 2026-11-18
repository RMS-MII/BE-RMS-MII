using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class BatchStageFlowDto
{
    public Guid Id { get; set; }
    public Guid StageTypeId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public StageCategory StageCategory { get; set; }
    public BatchStageLifecycleStatus LifecycleStatus { get; set; }
    public int CandidateCount { get; set; }
    public decimal? PassingScore { get; set; }
    public DateTime EndDate { get; set; }
}
