using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class BatchApplication : BaseEntity
{
    public string CodeApplication { get; set; } =  string.Empty;
    public DateTime AppliedAt { get; set; }
    public  string FinalResult { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public OfferingLetterStatus OfferingLetterStatus { get; set; } = OfferingLetterStatus.Pending;
    public string FileOfferingLetter {get; set;} = string.Empty;
    public Guid CandidateId { get; set; }

    public Candidate Candidate { get; set; } = null!;
    public ICollection<ApplicationStage> ApplicationStages { get; set; } = null!;
}