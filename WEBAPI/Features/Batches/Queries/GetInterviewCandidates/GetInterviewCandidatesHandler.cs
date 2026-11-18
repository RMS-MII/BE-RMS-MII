using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetInterviewCandidates;

public sealed class GetInterviewCandidatesHandler(RmsDbContext db)
    : IRequestHandler<GetInterviewCandidatesQuery, Result<IReadOnlyCollection<InterviewCandidateOptionDto>>>
{
    public async Task<Result<IReadOnlyCollection<InterviewCandidateOptionDto>>> Handle(
        GetInterviewCandidatesQuery request, CancellationToken ct)
    {
        if (!await db.RecruitmentBatches.AsNoTracking()
                .AnyAsync(batch => batch.Id == request.BatchId, ct))
            return Result<IReadOnlyCollection<InterviewCandidateOptionDto>>.Failure(
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
            .Select(application => new InterviewCandidateOptionDto
            {
                ApplicationId = application.Id,
                CodeApplication = application.CodeApplication,
                CandidateName = application.Candidate.FullName,
                CandidateEmail = application.Candidate.Email
            })
            .Take(5)
            .ToListAsync(ct);

        return Result<IReadOnlyCollection<InterviewCandidateOptionDto>>.Success(candidates);
    }
}
