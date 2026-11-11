using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Interviewers.ConfirmSession;

public class ConfirmSessionHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<ConfirmSessionHandler> logger) : IRequestHandler<ConfirmSessionCommand, Result<ConfirmSessionResponse>>
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

            await db.SaveChangesAsync(ct);

            // 6. Kirim email undangan ke kandidat (Aturan Bisnis #7)
            var candidate = session.ApplicationStage.BatchApplication.Candidate;
            var stageName = session.ApplicationStage.BatchStage.StageName;
            var portalUrl = EmailTemplates.GetCandidatePortalUrl(configuration, candidate.Id);
            var candidatePortalLink = $"{portalUrl}/sessions/{session.Id}";

            var content = EmailTemplates.CandidateInterviewInvitation(
                candidate.FullName,
                stageName,
                session.SessionName,
                session.ScheduledStart,
                session.ScheduledEnd,
                session.IsOnline,
                session.MeetingUrl,
                session.Location,
                candidatePortalLink);

            try
            {
                await emailService.SendEmailAsync(
                    candidate.Email,
                    content,
                    EmailType.InterviewInvitation,
                    ct);

                emailSent = true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Gagal mengirim email undangan wawancara untuk session {SessionId}",
                    session.Id);
            }
        }

        var message = emailSent
            ? "Kehadiran sesi wawancara berhasil dikonfirmasi dan email undangan resmi telah dikirimkan ke kandidat."
            : alreadyConfirmed
                ? "Kehadiran sesi wawancara sudah pernah dikonfirmasi sebelumnya."
                : "Kehadiran sesi wawancara berhasil dikonfirmasi, tetapi email undangan gagal dikirim.";

        return Result<ConfirmSessionResponse>.Success(
            sessionInterview.ToConfirmSessionResponse(emailSent, message));
    }
}
