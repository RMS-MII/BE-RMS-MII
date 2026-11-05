using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Interviewers.AssignTakeHome;

public class AssignTakeHomeHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<AssignTakeHomeCommand, Result<AssignTakeHomeResponse>>
{
    public async Task<Result<AssignTakeHomeResponse>> Handle(
        AssignTakeHomeCommand command,
        CancellationToken ct)
    {
        var currentEmployeeId = currentUser.EmployeeId;
        if (currentEmployeeId is null || currentEmployeeId == Guid.Empty)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.Forbidden("Akun Anda tidak terhubung dengan profil pegawai pewawancara."));
        }

        // 1. Ambil InterviewSession beserta relasi ApplicationStage, Candidate, SessionInterviews
        var session = await db.InterviewSessions
            .AsSplitQuery()
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchStage)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
            .FirstOrDefaultAsync(s => s.Id == command.InterviewSessionId, ct);

        if (session is null)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.InterviewSessionId}' tidak ditemukan."));
        }

        // 2. Validasi pewawancara ditugaskan pada sesi ini
        var isAssigned = session.SessionInterviews.Any(si =>
            si.StageEmployee.EmployeeId == currentEmployeeId.Value && si.Status != SessionInterviewStatus.Cancelled);

        if (!isAssigned)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.Forbidden("Anda tidak memiliki hak akses untuk memberikan tugas take-home pada sesi wawancara ini."));
        }

        // 3. Validasi status sesi
        if (session.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.BusinessRule("Tidak dapat menetapkan tugas take-home karena sesi ini telah dijadwalkan ulang."));
        }

        var now = DateTime.UtcNow;

        // 4. Update batas waktu pengumpulan tugas pada sesi
        session.EndedSubmissionAt = command.EndedSubmissionAt;
        session.UpdatedAt = now;

        // 5. Kirim email instruksi take-home test kepada kandidat (Aturan Bisnis #9)
        var candidate = session.ApplicationStage.BatchApplication.Candidate;
        var stageName = session.ApplicationStage.BatchStage.StageName;
        var frontendUrl = configuration["App:FrontendUrl"] ?? "https://localhost:5173";
        var submissionPortalLink = $"{frontendUrl}/portal/{candidate.Id}/sessions/{session.Id}/submission";

        var emailSubject = $"Instruksi Take-Home Test: {command.TaskTitle} - {stageName}";
        var emailBody =
            $"Halo {candidate.FullName},\n\n" +
            $"Pewawancara telah menetapkan tugas Take-Home Test untuk tahapan '{stageName}' dengan rincian sebagai berikut:\n\n" +
            $"Judul Tugas: {command.TaskTitle}\n" +
            $"Batas Waktu Pengumpulan: {command.EndedSubmissionAt:dd MMMM yyyy HH:mm} WIB\n\n" +
            $"Instruksi Pengerjaan:\n" +
            $"{command.Instructions}\n\n" +
            $"Silakan unggah berkas hasil pengerjaan Anda melalui portal kandidat sebelum batas waktu berakhir pada tautan berikut:\n" +
            $"{submissionPortalLink}\n\n" +
            $"Salam,\n" +
            $"Tim Rekrutmen";

        await db.SaveChangesAsync(ct);

        await emailService.SendEmailAsync(
            candidate.Email,
            emailSubject,
            emailBody,
            EmailType.TakeHome,
            ct);

        return Result<AssignTakeHomeResponse>.Success(
            session.ToAssignTakeHomeResponse(true, "Tugas take-home test berhasil ditetapkan dan instruksi telah dikirimkan ke email kandidat."));
    }
}
