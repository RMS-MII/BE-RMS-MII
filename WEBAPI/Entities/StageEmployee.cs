using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class StageEmployee : BaseEntity
{
    public Guid BatchStageId { get; set; }
    public Guid EmployeeId { get; set; }
    public StageEmployeeStatus Status { get; set; } = StageEmployeeStatus.Pending;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }

    // Navigation properties
    public BatchStage BatchStage { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ICollection<SessionInterview> SessionInterviews { get; set; } = [];
}
