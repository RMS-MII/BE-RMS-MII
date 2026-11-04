using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class SessionInterview : BaseEntity
{
    public Guid StageEmployeeId { get; set; }
    public Guid InterviewSessionId { get; set; }
    public SessionInterviewStatus Status { get; set; } = SessionInterviewStatus.InProgress;

    // Navigation properties
    public StageEmployee StageEmployee { get; set; } = null!;
    public InterviewSession InterviewSession { get; set; } = null!;
    public InterviewAssessment? InterviewAssessment { get; set; }
}
