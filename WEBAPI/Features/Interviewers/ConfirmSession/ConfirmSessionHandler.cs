using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Interviewers.ConfirmSession;

public class ConfirmSessionHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<ConfirmSessionCommand, Result<ConfirmSessionResponse>>
{
    public async Task<Result<ConfirmSessionResponse>> Handle(
        ConfirmSessionCommand command,
        CancellationToken ct)
    {
        // 1. Validasi identitas interviewer dari JWT
        var currentEmployeeId = currentUser.EmployeeId;
        if (currentEmployeeId is null || currentEmployeeId == Guid.Empty)
        {
            return Result<ConfirmSessionResponse>.Failure(
                Error.Forbidden("Akun Anda tidak terhubung dengan profil pegawai pewawancara."));
        }

        // 2. Ambil SessionInterview beserta relasi InterviewSession, ApplicationStage, Candidate, Stage
        var sessionInterview = await db.SessionInterviews
            .AsSplitQuery()
            .Include(si => si.StageEmployee)
                .ThenInclude(se => se.Employee)
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchStage)
            .Include(si => si.InterviewSession)
                .ThenInclude(s => s.ApplicationStage)
                    .ThenInclude(ast => ast.BatchApplication)
                        .ThenInclude(ba => ba.Candidate)
            .FirstOrDefaultAsync(si => si.Id == command.SessionInterviewId, ct);

        if (sessionInterview is null)
        {
            return Result<ConfirmSessionResponse>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.SessionInterviewId}' tidak ditemukan."));
        }

        // 3. Validasi kepemilikan: hanya pewawancara yang ditugaskan pada sesi ini yang boleh konfirmasi
        if (sessionInterview.StageEmployee.EmployeeId != currentEmployeeId.Value)
        {
            return Result<ConfirmSessionResponse>.Failure(
                Error.Forbidden("Anda tidak memiliki hak akses untuk mengonfirmasi sesi wawancara milik pewawancara lain."));
        }

        // 4. Validasi status sesi
        var session = sessionInterview.InterviewSession;

        if (sessionInterview.Status == SessionInterviewStatus.Cancelled || session.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<ConfirmSessionResponse>.Failure(
                Error.BusinessRule("Sesi wawancara ini sudah dibatalkan atau telah dijadwalkan ulang."));
        }

        if (sessionInterview.Status == SessionInterviewStatus.Completed)
        {
            return Result<ConfirmSessionResponse>.Failure(
                Error.BusinessRule("Sesi wawancara ini sudah selesai dilaksanakan."));
        }

        // 5. Cek apakah sudah pernah dikonfirmasi sebelumnya
        bool alreadyConfirmed = session.Status is InterviewSessionStatus.AcceptedByInterviewer
                                             or InterviewSessionStatus.AcceptedByCandidate
                                             or InterviewSessionStatus.Scheduled;

        bool emailSent = false;
        var now = DateTime.UtcNow;

        if (!alreadyConfirmed)
        {
            // Update status sesi menjadi AcceptedByInterviewer
            session.Status = InterviewSessionStatus.AcceptedByInterviewer;
            session.UpdatedAt = now;

            // 6. Kirim email undangan ke kandidat (Aturan Bisnis #7)
            var candidate = session.ApplicationStage.BatchApplication.Candidate;
            var stageName = session.ApplicationStage.BatchStage.StageName;
            var modeText = session.IsOnline
                ? $"Daring melalui tautan meeting: {session.MeetingUrl}"
                : $"Tatap Muka di lokasi: {session.Location}";

            var frontendUrl = configuration["App:FrontendUrl"] ?? "https://localhost:5173";
            var candidatePortalLink = $"{frontendUrl}/portal/{candidate.Id}/sessions/{session.Id}";

            var emailBody =
                $"Halo {candidate.FullName},\n\n" +
                $"Selamat! Anda diundang untuk mengikuti sesi wawancara pada tahapan '{stageName}'.\n\n" +
                $"Rincian Jadwal Wawancara:\n" +
                $"- Nama Sesi: {session.SessionName}\n" +
                $"- Waktu: {session.ScheduledStart:dd MMMM yyyy HH:mm} - {session.ScheduledEnd:HH:mm} WIB\n" +
                $"- Moda: {modeText}\n\n" +
                $"Silakan konfirmasi kehadiran Anda melalui portal kandidat pada tautan berikut:\n" +
                $"{candidatePortalLink}\n\n" +
                $"Salam,\n" +
                $"Tim Rekrutmen";

            await emailService.SendEmailAsync(
                candidate.Email,
                $"Undangan Sesi Wawancara - {session.SessionName}",
                emailBody,
                EmailType.InterviewInvitation,
                ct);

            emailSent = true;
            await db.SaveChangesAsync(ct);
        }

        var message = emailSent
            ? "Kehadiran sesi wawancara berhasil dikonfirmasi dan email undangan resmi telah dikirimkan ke kandidat."
            : "Kehadiran sesi wawancara sudah pernah dikonfirmasi sebelumnya.";

        return Result<ConfirmSessionResponse>.Success(
            sessionInterview.ToConfirmSessionResponse(emailSent, message));
    }
}
