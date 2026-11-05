using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;

public class RequestRescheduleHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<RequestRescheduleHandler> logger) : IRequestHandler<RequestRescheduleCommand, Result<RescheduleRequestDto>>
{
    public async Task<Result<RescheduleRequestDto>> Handle(
        RequestRescheduleCommand command,
        CancellationToken ct)
    {
        var session = await db.InterviewSessions
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchStage)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
            .FirstOrDefaultAsync(s => s.Id == command.InterviewSessionId, ct);

        if (session is null)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.InterviewSessionId}' tidak ditemukan."));
        }

        // 1. Validasi status sesi
        if (session.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.BusinessRule("Sesi wawancara ini sudah pernah dijadwalkan ulang sebelumnya."));
        }

        string requesterEmail;
        string requesterName;

        // 2. Identifikasi pemohon (Kandidat via portal vs Pewawancara/HR via internal token)
        if (command.CandidateId.HasValue)
        {
            var candidate = session.ApplicationStage?.BatchApplication?.Candidate;
            if (candidate is null || candidate.Id != command.CandidateId.Value)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.Forbidden("Sesi wawancara ini bukan milik kandidat yang bersangkutan."));
            }

            requesterEmail = candidate.Email;
            requesterName = candidate.FullName;
        }
        else
        {
            if (!currentUser.IsAuthenticated)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.Unauthorized("Autentikasi diperlukan untuk mengajukan reschedule."));
            }

            var isHr = currentUser.IsInRole(RmsRoles.Hr);
            var isAssignedInterviewer = session.SessionInterviews.Any(si =>
                si.StageEmployee.EmployeeId == currentUser.EmployeeId);

            if (!isHr && !isAssignedInterviewer)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.Forbidden("Anda tidak memiliki hak akses untuk mengajukan permohonan reschedule pada sesi wawancara ini."));
            }

            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == currentUser.EmployeeId, ct);
            if (employee is null)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.NotFound("Profil data pegawai pemohon tidak ditemukan."));
            }

            requesterEmail = employee.Email;
            requesterName = employee.FullName;
        }

        // 3. Cegah duplikasi permohonan pending pada sesi yang sama
        var hasPendingRequest = await db.RescheduleRequests.AnyAsync(
            r => r.InterviewSessionId == command.InterviewSessionId && r.Status == RescheduleStatus.Pending, ct);

        if (hasPendingRequest)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.Conflict("Masih terdapat permohonan reschedule yang berstatus Pending untuk sesi wawancara ini."));
        }

        // 4. Simpan permohonan baru
        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        var rescheduleRequest = new RescheduleRequest
        {
            Id = Guid.CreateVersion7(),
            InterviewSessionId = session.Id,
            RequesterEmail = requesterEmail,
            ProposedStart = command.ProposedStart,
            ProposedEnd = command.ProposedEnd,
            Reason = command.Reason.Trim(),
            Status = RescheduleStatus.Pending,
            CreatedAt = now
        };

        db.RescheduleRequests.Add(rescheduleRequest);
        await db.SaveChangesAsync(ct);

        // 5. Kirim email notifikasi permohonan reschedule
        await SendRescheduleRequestNotificationAsync(rescheduleRequest, session, requesterName, ct);

        return Result<RescheduleRequestDto>.Success(rescheduleRequest.ToDto(session));
    }

    private async Task SendRescheduleRequestNotificationAsync(
        RescheduleRequest request,
        InterviewSession session,
        string requesterName,
        CancellationToken ct)
    {
        try
        {
            var stageName = session.ApplicationStage?.BatchStage?.StageName ?? "Wawancara";
            var subject = $"Permohonan Reschedule Sesi Wawancara - {session.SessionName}";
            var body =
                $"""
                Halo {requesterName},

                Permohonan penjadwalan ulang (reschedule) untuk sesi '{session.SessionName}' pada tahapan '{stageName}' telah kami terima.

                Detail Pengajuan:
                - Jadwal Semula: {session.ScheduledStart:dd MMM yyyy HH:mm} - {session.ScheduledEnd:HH:mm} WIB
                - Usulan Jadwal Baru: {request.ProposedStart:dd MMM yyyy HH:mm} - {request.ProposedEnd:HH:mm} WIB
                - Alasan: {request.Reason}
                - Status: Menunggu Peninjauan HR

                Tim HR akan segera meninjau permohonan Anda dan memberikan konfirmasi melalui email.

                Terima kasih,
                Tim Rekrutmen
                """;

            await emailService.SendEmailAsync(
                request.RequesterEmail,
                subject,
                body,
                EmailType.RescheduleRequest,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim notifikasi email permohonan reschedule untuk request ID {RequestId}",
                request.Id);
        }
    }
}

