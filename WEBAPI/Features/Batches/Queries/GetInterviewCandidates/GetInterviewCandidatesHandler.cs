using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetInterviewCandidates;

public sealed class GetInterviewCandidatesHandler(RmsDbContext db)
    : IRequestHandler<GetInterviewCandidatesQuery, Result<InterviewCandidatesPageDto>>
{
    public async Task<Result<InterviewCandidatesPageDto>> Handle(
        GetInterviewCandidatesQuery request, CancellationToken ct)
    {
        const int pageSize = 5;
        if (request.Page < 1 || (long)(request.Page - 1) * pageSize > int.MaxValue)
            return Result<InterviewCandidatesPageDto>.Failure(
                Error.Validation("Page must be a positive number."));

        if (!await db.RecruitmentBatches.AsNoTracking()
                .AnyAsync(batch => batch.Id == request.BatchId, ct))
            return Result<InterviewCandidatesPageDto>.Failure(
                Error.NotFound("Batch was not found."));

        var query = db.BatchApplications.AsNoTracking()
            .Where(application => application.RecruitmentBatchId == request.BatchId
                && application.Status == ApplicationStatus.InProcess
                && application.ApplicationStages.Any(stage =>
                    stage.Status == ApplicationStageStatus.InProgress
                    && stage.BatchStage.StageType.StageCategory == StageCategory.Interview
                    && (application.IsStandalone ||
                        stage.BatchStage.LifecycleStatus == BatchStageLifecycleStatus.Active)
                    && !stage.InterviewSessions.Any(session =>
                        session.Status != InterviewSessionStatus.Rescheduled
                        && session.SessionInterviews.Any(interviewer =>
                            interviewer.Status != SessionInterviewStatus.Cancelled))));

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(application =>
                application.Candidate.FullName.Contains(search)
                || application.Candidate.Email.Contains(search)
                || application.CodeApplication.Contains(search));

        var candidates = await query
            .OrderBy(application => application.Candidate.FullName)
            .ThenBy(application => application.Id)
            .Skip((request.Page - 1) * pageSize)
            .Select(application => new InterviewCandidateOptionDto
            {
                ApplicationId = application.Id,
                CodeApplication = application.CodeApplication,
                CandidateName = application.Candidate.FullName,
                CandidateEmail = application.Candidate.Email
            })
            .Take(pageSize + 1)
            .ToListAsync(ct);

        return Result<InterviewCandidatesPageDto>.Success(new InterviewCandidatesPageDto
        {
            Items = candidates.Take(pageSize).ToList(),
            HasMore = candidates.Count > pageSize
        });
    }
}
