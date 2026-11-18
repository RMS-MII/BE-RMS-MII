namespace WEBAPI.Models;

public class GetBatchDashboardDto
{
    public int TotalApplications { get; set; }
    public int InProcessApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int DroppedApplications { get; set; }
    public int EmployeeCount { get; set; }
}
