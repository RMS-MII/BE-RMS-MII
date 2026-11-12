using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class BatchApplication : BaseEntity
{
    public string CodeApplication { get; set; } =  string.Empty;
    public DateTime AppliedAt { get; set; }
    public FinalResultStatus? FinalResultStatus { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.InProcess;
    public string Notes { get; set; } = string.Empty;
    public OfferingLetterStatus OfferingLetterStatus { get; set; } = OfferingLetterStatus.Pending;
    public string FileOfferingLetter { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public bool IsStandalone { get; set; }

    public Candidate Candidate { get; set; } = null!;
    public RecruitmentBatch RecruitmentBatch { get; set; } = null!;
    public ICollection<ApplicationStage> ApplicationStages { get; set; } = new List<ApplicationStage>();
}
