using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.SubmitAssessment;

public class SubmitAssessmentHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<SubmitAssessmentCommand, Result<AssessmentDto>>
{
    public async Task<Result<AssessmentDto>> Handle(
        SubmitAssessmentCommand command,
        CancellationToken ct)
    {
        var currentEmployeeId = currentUser.EmployeeId;
        if (currentEmployeeId is null || currentEmployeeId == Guid.Empty)
        {
            return Result<AssessmentDto>.Failure(
                Error.Forbidden("Akun Anda tidak terhubung dengan profil pegawai pewawancara."));
        }

        // 1. Ambil SessionInterview beserta relasi InterviewSession, ApplicationStage, Candidate, Assessment
        var sessionInterview = await db.SessionInterviews
            .Include(si => si.StageEmployee)
                .ThenInclude(se => se.Employee)
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchApplication)
                        .ThenInclude(ba => ba.Candidate)
            .Include(si => si.InterviewAssessment)
            .FirstOrDefaultAsync(si => si.Id == command.SessionInterviewId, ct);

        if (sessionInterview is null)
        {
            return Result<AssessmentDto>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.SessionInterviewId}' tidak ditemukan."));
        }

        // 2. Validasi kepemilikan sesi
        if (sessionInterview.StageEmployee.EmployeeId != currentEmployeeId.Value)
        {
            return Result<AssessmentDto>.Failure(
                Error.Forbidden("Anda tidak memiliki hak akses untuk memberikan penilaian pada sesi wawancara pewawancara lain."));
        }

        // 3. Validasi status sesi
        if (sessionInterview.Status == SessionInterviewStatus.Cancelled || sessionInterview.InterviewSession.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<AssessmentDto>.Failure(
                Error.BusinessRule("Tidak dapat memberikan penilaian karena sesi wawancara ini telah dibatalkan atau dijadwalkan ulang."));
        }

        var now = DateTime.UtcNow;
        var assessment = sessionInterview.InterviewAssessment;

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

        await db.SaveChangesAsync(ct);

        return Result<AssessmentDto>.Success(assessment.ToAssessmentDto(sessionInterview));
    }
}
