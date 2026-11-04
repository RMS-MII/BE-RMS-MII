using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Queries.GetApplicationRecap;

public sealed class GetApplicationRecapHandler(RmsDbContext db)
    : IRequestHandler<GetApplicationRecapQuery, Result<ApplicationRecapDto>>
{
    public async Task<Result<ApplicationRecapDto>> Handle(
        GetApplicationRecapQuery request, CancellationToken ct)
    {
        var application = await db.BatchApplications
            .AsNoTracking()
            .AsSplitQuery()
            .Include(application => application.Candidate)
            .Include(application => application.RecruitmentBatch)
                .ThenInclude(batch => batch.Department)
            .Include(application => application.RecruitmentBatch)
                .ThenInclude(batch => batch.BatchStages)
                    .ThenInclude(stage => stage.StageType)
            .Include(application => application.RecruitmentBatch)
                .ThenInclude(batch => batch.BatchStages)
                    .ThenInclude(stage => stage.StageEmployees)
                        .ThenInclude(stageEmployee => stageEmployee.Employee)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.InterviewSessions)
                    .ThenInclude(session => session.SessionInterviews)
                        .ThenInclude(sessionInterview => sessionInterview.StageEmployee)
                            .ThenInclude(stageEmployee => stageEmployee.Employee)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.InterviewSessions)
                    .ThenInclude(session => session.SessionInterviews)
                        .ThenInclude(sessionInterview => sessionInterview.InterviewAssessment)
            .Include(application => application.ApplicationStages)
                .ThenInclude(stage => stage.CandidateAnswers)
            .FirstOrDefaultAsync(application => application.Id == request.Id, ct);

        if (application is null)
            return Result<ApplicationRecapDto>.Failure(
                Error.NotFound("Application was not found."));

        return Result<ApplicationRecapDto>.Success(application.ToRecapDto());
    }
}
