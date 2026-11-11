namespace WEBAPI.Models;

public class GetBatchDashboardDto
{
    public int TotalCandidates { get; set; }
    public int AcceptedCandidates { get; set; }
    public int NotPassedCandidates { get; set; }
    public int EmployeeCount { get; set; }
}
