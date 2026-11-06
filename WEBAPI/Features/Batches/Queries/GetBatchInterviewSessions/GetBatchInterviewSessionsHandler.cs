using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetBatchInterviewSessions;

public sealed class GetBatchInterviewSessionsHandler(RmsDbContext db)
    : IRequestHandler<GetBatchInterviewSessionsQuery, Result<IReadOnlyCollection<InterviewSessionDto>>>
{
    public async Task<Result<IReadOnlyCollection<InterviewSessionDto>>> Handle(
        GetBatchInterviewSessionsQuery request, CancellationToken ct)
    {
        var batchExists = await db.RecruitmentBatches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BatchId, ct);

        if (!batchExists)
            return Result<IReadOnlyCollection<InterviewSessionDto>>.Failure(
                Error.NotFound("Batch was not found."));

        var query = db.InterviewSessions
            .AsNoTracking()
            .Include(s => s.ApplicationStage)
                .ThenInclude(stage => stage.BatchApplication)
                    .ThenInclude(app => app.Candidate)
            .Include(s => s.ApplicationStage)
                .ThenInclude(stage => stage.BatchStage)
                    .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
                        .ThenInclude(e => e.Department)
            .Where(s => s.ApplicationStage.BatchApplication.RecruitmentBatchId == request.BatchId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.SessionName.ToLower().Contains(search) ||
                s.ApplicationStage.BatchApplication.Candidate.FullName.ToLower().Contains(search) ||
                s.ApplicationStage.BatchApplication.Candidate.Email.ToLower().Contains(search));
        }

        if (request.StageId.HasValue && request.StageId.Value != Guid.Empty)
        {
            query = query.Where(s => s.ApplicationStage.BatchStageId == request.StageId.Value);
        }

        if (request.IsOnline.HasValue)
        {
            query = query.Where(s => s.IsOnline == request.IsOnline.Value);
        }

        if (request.Date.HasValue)
        {
            var date = request.Date.Value;
            var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(s => s.ScheduledStart >= startOfDay && s.ScheduledStart <= endOfDay);
        }

        var sessions = await query
            .OrderBy(s => s.ScheduledStart)
            .ToListAsync(ct);

        var sessionIds = sessions.Select(s => s.Id).ToList();

        var pendingRescheduleSessionIds = await db.RescheduleRequests
            .AsNoTracking()
            .Where(r => sessionIds.Contains(r.InterviewSessionId) && r.Status == RescheduleStatus.Pending)
            .Select(r => r.InterviewSessionId)
            .Distinct()
            .ToListAsync(ct);

        var pendingSet = pendingRescheduleSessionIds.ToHashSet();

        var dtoList = sessions
            .Select(s => s.ToInterviewSessionDto(pendingSet.Contains(s.Id)))
            .ToList();

        return Result<IReadOnlyCollection<InterviewSessionDto>>.Success(dtoList);
    }
}

