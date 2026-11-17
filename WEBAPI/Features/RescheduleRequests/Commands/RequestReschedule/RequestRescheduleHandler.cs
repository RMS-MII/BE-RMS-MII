using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

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
                    .ThenInclude(batchStage => batchStage.RecruitmentBatch)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
            .FirstOrDefaultAsync(s => s.Id == command.InterviewSessionId, ct);

        if (session?.ApplicationStage is not { } stage)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.InterviewSessionId}' tidak ditemukan."));
        }

        if (stage.BatchStage.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<RescheduleRequestDto>.Failure(
                Error.BusinessRule("A closed recruitment batch cannot be edited."));

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
            var candidate = stage.BatchApplication?.Candidate;
            if (candidate is null || candidate.Id != command.CandidateId.Value)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.Forbidden("Sesi wawancara ini bukan milik kandidat yang bersangkutan."));
            }

            if (!WEBAPI.Features.Portal.CandidateActionPolicy.CanReschedule(session, dateTimeProvider.GetUtcNow().UtcDateTime))
                return Result<RescheduleRequestDto>.Failure(Error.BusinessRule("Pengajuan hanya tersedia setelah interviewer menerima jadwal, sebelum sesi dimulai, dan selama tahap masih aktif."));

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
            var isSessionLead = session.SessionInterviews.Any(si =>
                si.StageEmployee.EmployeeId == currentUser.EmployeeId &&
                si.Status != SessionInterviewStatus.Cancelled &&
                si.IsLead);

            if (!isHr && !isSessionLead)
            {
                return Result<RescheduleRequestDto>.Failure(
                    Error.Forbidden("Reschedule hanya dapat diajukan oleh Lead Interviewer pada sesi ini atau HR."));
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

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        if (!WEBAPI.Features.Portal.CandidateActionPolicy.IsActive(stage) ||
            session.ScheduledStart <= now ||
            session.SessionInterviews.Any(interviewer => interviewer.Status == SessionInterviewStatus.Completed))
            return Result<RescheduleRequestDto>.Failure(
                Error.BusinessRule("Reschedule hanya dapat diajukan sebelum interview dimulai dan selama tahap masih aktif."));

        // 3. Cegah duplikasi permohonan pending pada sesi yang sama
        var hasPendingRequest = await db.RescheduleRequests.AnyAsync(
            r => r.InterviewSessionId == command.InterviewSessionId && r.Status == RescheduleStatus.Pending, ct);

        if (hasPendingRequest)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.Conflict("Masih terdapat permohonan reschedule yang berstatus Pending untuk sesi wawancara ini."));
        }

        // 4. Simpan permohonan baru
        var rescheduleRequest = new RescheduleRequest
        {
            Id = Guid.CreateVersion7(),
            InterviewSessionId = session.Id,
            RequesterEmail = requesterEmail,
            RequestedByCandidate = command.CandidateId.HasValue,
            ProposedStart = command.ProposedStartUtc,
            ProposedEnd = command.ProposedEndUtc,
            Reason = command.Reason.Trim(),
            Status = RescheduleStatus.Pending,
            CreatedAt = now
        };

        db.RescheduleRequests.Add(rescheduleRequest);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.Conflict("Masih terdapat permohonan reschedule yang berstatus Pending untuk sesi wawancara ini."));
        }

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
            var content = EmailTemplates.RescheduleRequestReceived(
                requesterName,
                session.SessionName,
                stageName,
                session.ScheduledStart,
                session.ScheduledEnd,
                request.ProposedStart,
                request.ProposedEnd,
                request.Reason);

            await emailService.SendEmailAsync(
                request.RequesterEmail,
                content,
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

