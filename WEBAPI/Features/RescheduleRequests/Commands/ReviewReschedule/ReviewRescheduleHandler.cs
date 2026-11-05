using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;

public class ReviewRescheduleHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<ReviewRescheduleHandler> logger) : IRequestHandler<ReviewRescheduleCommand, Result<RescheduleRequestDto>>
{
    public async Task<Result<RescheduleRequestDto>> Handle(
        ReviewRescheduleCommand command,
        CancellationToken ct)
    {
        var request = await db.RescheduleRequests
            .FirstOrDefaultAsync(r => r.Id == command.RescheduleRequestId, ct);

        if (request is null)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.NotFound($"Permohonan reschedule dengan ID '{command.RescheduleRequestId}' tidak ditemukan."));
        }

        // 1. Validasi transisi status (hanya status Pending yang dapat di-review)
        if (!request.Status.CanReview(command.Decision))
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.BusinessRule("Permohonan reschedule ini sudah ditinjau sebelumnya dan tidak dapat diubah kembali."));
        }

        // 2. Ambil sesi wawancara terkait beserta relasinya
        var session = await db.InterviewSessions
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchStage)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
            .FirstOrDefaultAsync(s => s.Id == request.InterviewSessionId, ct);

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        var reviewerId = currentUser.EmployeeId ?? currentUser.AccountId;

        // 3. Update status permohonan reschedule
        request.Status = command.Decision;
        request.ReviewedBy = reviewerId;
        request.ReviewedAt = now;
        request.ReviewNotes = command.ReviewNotes?.Trim();
        request.UpdatedAt = now;

        // 4. Logika berdasarkan keputusan (Aturan Bisnis #8)
        if (command.Decision == RescheduleStatus.Approved)
        {
            // Sesi lama berstatus Rescheduled
            if (session is not null)
            {
                session.Status = InterviewSessionStatus.Rescheduled;
                session.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(ct);

        // 5. Kirim email notifikasi hasil keputusan reschedule (EmailType.RescheduleResult)
        if (session is not null)
        {
            await SendDecisionEmailAsync(request, session, ct);
        }

        return Result<RescheduleRequestDto>.Success(request.ToDto(session));
    }

    private async Task SendDecisionEmailAsync(
        RescheduleRequest request,
        InterviewSession session,
        CancellationToken ct)
    {
        try
        {
            var isApproved = request.Status == RescheduleStatus.Approved;
            var decisionText = isApproved ? "DISETUJUI" : "DITOLAK";
            var subject = $"Hasil Peninjauan Reschedule: {decisionText} - {session.SessionName}";
            var stageName = session.ApplicationStage?.BatchStage?.StageName ?? "Wawancara";

            var explanation = isApproved
                ? "Permohonan reschedule Anda telah DISETUJUI oleh HR. Sesi wawancara sebelumnya telah dibatalkan/dijadwalkan ulang, dan tim HR akan segera menerbitkan undangan dengan jadwal baru."
                : $"Permohonan reschedule Anda DITOLAK oleh HR. Jadwal wawancara semula ({session.ScheduledStart:dd MMM yyyy HH:mm} - {session.ScheduledEnd:HH:mm} WIB) tetap berlaku.";

            var notesLine = !string.IsNullOrWhiteSpace(request.ReviewNotes)
                ? $"\nCatatan HR: {request.ReviewNotes}\n"
                : string.Empty;

            var body =
                $"""
                Halo,

                Berikut adalah hasil peninjauan permohonan reschedule sesi wawancara '{session.SessionName}' pada tahapan '{stageName}':

                Keputusan: {decisionText}
                {notesLine}
                Keterangan:
                {explanation}

                Salam,
                Tim Rekrutmen
                """;

            await emailService.SendEmailAsync(
                request.RequesterEmail,
                subject,
                body,
                EmailType.RescheduleResult,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirimkan email keputusan reschedule untuk request ID {RequestId}",
                request.Id);
        }
    }
}

