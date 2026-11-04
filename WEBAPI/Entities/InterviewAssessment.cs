using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class InterviewAssessment : BaseEntity
{
    public Guid SessionInterviewId { get; set; }
    public string? Notes { get; set; }
    public Recommendation Recommendation { get; set; }
    public decimal Score { get; set; }

    // Navigation properties
    public SessionInterview SessionInterview { get; set; } = null!;
}
