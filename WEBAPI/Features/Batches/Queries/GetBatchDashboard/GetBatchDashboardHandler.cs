using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchDashboard;

public sealed class GetBatchDashboardHandler(RmsDbContext db)
    : IRequestHandler<GetBatchDashboardQuery, Result<GetBatchDashboardDto>>
{
    public async Task<Result<GetBatchDashboardDto>> Handle(
        GetBatchDashboardQuery request,
        CancellationToken ct)
    {
        var counts = await db.BatchApplications.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                InProcess = group.Count(application => application.Status == ApplicationStatus.InProcess),
                Accepted = group.Count(application => application.Status == ApplicationStatus.Accepted),
                Rejected = group.Count(application => application.Status == ApplicationStatus.Rejected),
                Dropped = group.Count(application => application.Status == ApplicationStatus.Dropped)
            })
            .FirstOrDefaultAsync(ct);

        var employeeCount = await db.Employees.AsNoTracking()
            .CountAsync(employee => employee.IsActive, ct);

        var dto = new GetBatchDashboardDto
        {
            TotalApplications = counts?.Total ?? 0,
            InProcessApplications = counts?.InProcess ?? 0,
            AcceptedApplications = counts?.Accepted ?? 0,
            RejectedApplications = counts?.Rejected ?? 0,
            DroppedApplications = counts?.Dropped ?? 0,
            EmployeeCount = employeeCount
        };

        return Result<GetBatchDashboardDto>.Success(dto);
    }
}
