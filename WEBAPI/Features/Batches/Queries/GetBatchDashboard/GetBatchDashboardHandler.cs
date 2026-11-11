using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
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
        var totalCandidates = await db.BatchApplications
            .AsNoTracking()
            .CountAsync(ct);

        var acceptedCandidates = await db.BatchApplications
            .AsNoTracking()
            .CountAsync(
                application =>
                    application.Status == ApplicationStatus.Accepted,
                ct);

        var notPassedCandidates = await db.BatchApplications
            .AsNoTracking()
            .CountAsync(
                application =>
                    application.FinalResultStatus == FinalResultStatus.Rejected,
                ct);

        var employeeCount = await db.StageEmployees
            .AsNoTracking()
            .CountAsync(ct);

        var dto = new GetBatchDashboardDto
        {
            TotalCandidates = totalCandidates,
            AcceptedCandidates = acceptedCandidates,
            NotPassedCandidates = notPassedCandidates,
            EmployeeCount = employeeCount
        };

        return Result<GetBatchDashboardDto>.Success(dto);
    }
}
