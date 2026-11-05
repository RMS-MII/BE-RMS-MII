using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Portal.Queries.GetCandidateTasks;

public sealed class GetCandidateTasksHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetCandidateTasksQuery, Result<CandidateTasksDto>>
{
    public async Task<Result<CandidateTasksDto>> Handle(
        GetCandidateTasksQuery request, CancellationToken ct)
    {
        var candidate = await db.Candidates
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.CandidateId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.FullName,
                candidate.Email
            })
            .FirstOrDefaultAsync(ct);

        if (candidate is null)
            return Result<CandidateTasksDto>.Failure(
                Error.NotFound("Candidate was not found."));

        var applications = await db.BatchApplications
            .AsNoTracking()
            .AsSplitQuery()
            .Where(application => application.CandidateId == request.CandidateId)
            .Include(application => application.RecruitmentBatch)
                .ThenInclude(batch => batch.Department)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.BatchStage)
                    .ThenInclude(batchStage => batchStage.StageType)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.InterviewSessions)
            .OrderByDescending(application => application.AppliedAt)
            .ToListAsync(ct);

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        return Result<CandidateTasksDto>.Success(new CandidateTasksDto
        {
            CandidateId = candidate.Id,
            CandidateName = candidate.FullName,
            CandidateEmail = candidate.Email,
            Applications = applications
                .Select(application => application.ToCandidateApplicationTaskDto(now))
                .ToList()
        });
    }
}
