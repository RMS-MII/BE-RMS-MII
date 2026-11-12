using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetInterviewerSessionDetail;

public sealed class GetInterviewerSessionDetailHandler(
    RmsDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetInterviewerSessionDetailQuery, Result<InterviewerSessionDetailDto>>
{
    public async Task<Result<InterviewerSessionDetailDto>> Handle(
        GetInterviewerSessionDetailQuery request,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId;
        if (employeeId is null || employeeId == Guid.Empty)
            return Result<InterviewerSessionDetailDto>.Failure(
                Error.Unauthorized("Akun pewawancara tidak terhubung dengan profil pegawai."));

        var assignment = await db.SessionInterviews
            .AsNoTracking()
            .AsSplitQuery()
            .Include(si => si.StageEmployee)
            .Include(si => si.InterviewAssessment)
            .Include(si => si.InterviewSession)
                .ThenInclude(session => session.ApplicationStage)
                    .ThenInclude(stage => stage.BatchApplication)
            .Include(si => si.InterviewSession)
                .ThenInclude(session => session.ApplicationStage)
                    .ThenInclude(stage => stage.BatchStage)
                        .ThenInclude(stage => stage.RecruitmentBatch)
            .FirstOrDefaultAsync(si => si.Id == request.SessionInterviewId
                && si.StageEmployee.EmployeeId == employeeId.Value, ct);

        if (assignment is null)
            return Result<InterviewerSessionDetailDto>.Failure(
                Error.NotFound("Sesi wawancara tidak ditemukan untuk akun ini."));

        var session = assignment.InterviewSession;
        var panel = await db.SessionInterviews
            .AsNoTracking()
            .Include(si => si.StageEmployee)
                .ThenInclude(stageEmployee => stageEmployee.Employee)
            .Where(si => si.InterviewSessionId == session.Id)
            .ToListAsync(ct);
        session.SessionInterviews = panel;

        var candidateId = session.ApplicationStage.BatchApplication.CandidateId;
        var candidate = await db.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == candidateId, ct);
        if (candidate is null)
            return Result<InterviewerSessionDetailDto>.Failure(
                Error.NotFound("Profil kandidat untuk sesi wawancara ini tidak ditemukan."));

        var batch = session.ApplicationStage.BatchStage.RecruitmentBatch;
        var canAssess = assignment.Status != SessionInterviewStatus.Cancelled
            && session.Status == InterviewSessionStatus.AcceptedByCandidate
            && batch.Status != BatchStatus.Closed;

        return Result<InterviewerSessionDetailDto>.Success(new InterviewerSessionDetailDto
        {
            Session = assignment.ToDashboardInterviewSessionDto(),
            Candidate = candidate.ToDto(),
            AssessmentNotes = assignment.InterviewAssessment?.Notes,
            CanAssess = canAssess
        });
    }
}
