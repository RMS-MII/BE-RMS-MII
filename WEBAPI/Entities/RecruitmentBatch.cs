using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class RecruitmentBatch : BaseEntity
{
    public Guid DepartmentId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ApplicationDeadlineAt { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Draft;
    public Guid CreatedBy { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<BatchStage> BatchStages { get; set; } = [];
    public ICollection<BatchApplication> BatchApplications { get; set; } = [];
}
