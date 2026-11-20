using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Interviewers.SubmitAssessment;

public class SubmitAssessmentHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<SubmitAssessmentCommand, Result<AssessmentDto>>
{
    public async Task<Result<AssessmentDto>> Handle(
        SubmitAssessmentCommand command,
        CancellationToken ct)
    {
        var currentEmployeeId = currentUser.EmployeeId;
        if (currentEmployeeId is null || currentEmployeeId == Guid.Empty)
        {
            return Result<AssessmentDto>.Failure(
                Error.Forbidden("Your account is not linked to an interviewer employee profile."));
        }

        // 1. Ambil SessionInterview beserta relasi InterviewSession, ApplicationStage, Candidate, Assessment
        var sessionInterview = await db.SessionInterviews
            .Include(si => si.StageEmployee)
                .ThenInclude(se => se.Employee)
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchApplication)
                        .ThenInclude(ba => ba.Candidate)
            .Include(si => si.InterviewSession)
                .ThenInclude(session => session.ApplicationStage)
                    .ThenInclude(stage => stage.BatchStage)
                        .ThenInclude(stage => stage.RecruitmentBatch)
            .Include(si => si.InterviewAssessment)
            .FirstOrDefaultAsync(si => si.Id == command.SessionInterviewId, ct);

        if (sessionInterview is null)
        {
            return Result<AssessmentDto>.Failure(
                Error.NotFound($"Interview session with ID '{command.SessionInterviewId}' was not found."));
        }

        // 2. Validasi kepemilikan sesi
        if (sessionInterview.StageEmployee.EmployeeId != currentEmployeeId.Value)
        {
            return Result<AssessmentDto>.Failure(
                Error.Forbidden("You do not have permission to submit an assessment for another interviewer's session."));
        }

        // 3. Validasi status sesi
        if (sessionInterview.Status == SessionInterviewStatus.Cancelled || sessionInterview.InterviewSession.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<AssessmentDto>.Failure(
                Error.BusinessRule("An assessment cannot be submitted because this interview session has been cancelled or rescheduled."));
        }

        if (sessionInterview.InterviewSession.Status != InterviewSessionStatus.AcceptedByCandidate)
            return Result<AssessmentDto>.Failure(
                Error.BusinessRule("Assessment is available after the candidate confirms the interview schedule."));

        if (sessionInterview.InterviewSession.ApplicationStage.BatchStage.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<AssessmentDto>.Failure(
                Error.BusinessRule("A closed recruitment batch cannot be edited."));

        var now = DateTime.UtcNow;
        var assessment = sessionInterview.InterviewAssessment;
        var isUpdate = assessment is not null;

        // 4. Buat atau update penilaian wawancara (Aturan Bisnis #10)
        if (assessment is null)
        {
            assessment = new InterviewAssessment
            {
                Id = Guid.CreateVersion7(),
                SessionInterviewId = sessionInterview.Id,
                Score = command.Score,
                Recommendation = command.Recommendation,
                Notes = command.Notes?.Trim(),
                CreatedAt = now
            };
            db.InterviewAssessments.Add(assessment);
        }
        else
        {
            assessment.Score = command.Score;
            assessment.Recommendation = command.Recommendation;
            assessment.Notes = command.Notes?.Trim();
            assessment.UpdatedAt = now;
        }

        // 5. Tandai status pelaksanaan wawancara oleh pewawancara ini sebagai Completed
        sessionInterview.Status = SessionInterviewStatus.Completed;
        sessionInterview.UpdatedAt = now;

        var stage = sessionInterview.InterviewSession.ApplicationStage;
        var application = stage.BatchApplication;
        var batch = stage.BatchStage.RecruitmentBatch;
        var hr = await db.Employees.AsNoTracking().FirstOrDefaultAsync(employee =>
            employee.Id == batch.CreatedBy, ct);

        if (hr is not null && !string.IsNullOrWhiteSpace(hr.Email))
        {
            var applicationUrl = $"{EmailTemplates.GetFrontendUrl(configuration)}/recruitments/{batch.Id}/applications/{application.Id}";
            var content = EmailTemplates.InterviewAssessment(
                hr.FullName, sessionInterview.StageEmployee.Employee.FullName,
                application.Candidate.FullName, batch.Name, stage.BatchStage.StageName,
                isUpdate, applicationUrl);
            emailService.QueueBulkEmail([new BulkEmailMessage(hr.Email, content)], EmailType.InterviewAssessment);
        }

        await db.SaveChangesAsync(ct);

        return Result<AssessmentDto>.Success(assessment.ToAssessmentDto(sessionInterview));
    }
}
