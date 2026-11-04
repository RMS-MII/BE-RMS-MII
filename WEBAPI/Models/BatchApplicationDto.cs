using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class BatchApplicationDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public FinalResultStatus? FinalResultStatus { get; set; }
    public ApplicationStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public OfferingLetterStatus OfferingLetterStatus { get; set; }
    public string FileOfferingLetter { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CandidateDto? Candidate { get; set; }
}
