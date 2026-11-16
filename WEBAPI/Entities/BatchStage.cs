namespace WEBAPI.Entities;

public class BatchStage : BaseEntity
{
    public Guid? NextStageId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public Guid StageTypeId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public WEBAPI.Entities.Enums.BatchStageLifecycleStatus LifecycleStatus { get; set; } =
        WEBAPI.Entities.Enums.BatchStageLifecycleStatus.Inactive;
    public decimal? PassingScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? PlannedDurationTicks { get; set; }

    // Navigation properties
    public BatchStage? NextStage { get; set; }
    public ICollection<BatchStage> PreviousStages { get; set; } = [];
    public RecruitmentBatch RecruitmentBatch { get; set; } = null!;
    public StageType StageType { get; set; } = null!;
    public ICollection<ApplicationStage> ApplicationStages { get; set; } = [];
    public ICollection<StageEmployee> StageEmployees { get; set; } = [];
}
