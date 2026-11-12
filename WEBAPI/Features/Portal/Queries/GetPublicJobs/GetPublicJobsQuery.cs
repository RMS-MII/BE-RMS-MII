using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Queries.GetPublicJobs;

public sealed record GetPublicJobsQuery : IRequest<IReadOnlyList<PublicJobDto>>;

public sealed record PublicJobDto(
    Guid Id, string Name, string? Department, string Description, DateTime? ApplicationDeadlineAt);

public sealed class GetPublicJobsHandler(RmsDbContext db, IDateTimeProvider clock)
    : IRequestHandler<GetPublicJobsQuery, IReadOnlyList<PublicJobDto>>
{
    public async Task<IReadOnlyList<PublicJobDto>> Handle(GetPublicJobsQuery request, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return await db.RecruitmentBatches.AsNoTracking()
            .Where(PublicJobAvailability.AcceptsApplications(now))
            .OrderBy(batch => batch.ApplicationDeadlineAt ?? batch.EndDate)
            .ThenBy(batch => batch.Name)
            .ThenBy(batch => batch.Id)
            .Select(batch => new PublicJobDto(batch.Id, batch.Name, batch.Department.Name,
                batch.Description, batch.ApplicationDeadlineAt))
            .ToListAsync(ct);
    }
}
