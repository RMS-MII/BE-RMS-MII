using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;

public class ReviewRescheduleHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<ReviewRescheduleHandler> logger) : IRequestHandler<ReviewRescheduleCommand, Result<RescheduleRequestDto>>
{
    public async Task<Result<RescheduleRequestDto>> Handle(
        ReviewRescheduleCommand command,
        CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
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
                    .ThenInclude(batchStage => batchStage.RecruitmentBatch)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .Include(s => s.SessionInterviews)
                .ThenInclude(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
            .FirstOrDefaultAsync(s => s.Id == request.InterviewSessionId, ct);

        if (session is null)
        {
            return Result<RescheduleRequestDto>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{request.InterviewSessionId}' tidak ditemukan."));
        }

        if (session.ApplicationStage.BatchStage.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<RescheduleRequestDto>.Failure(
                Error.BusinessRule("Closed recruitment interview schedules cannot be changed."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        var reviewerId = currentUser.EmployeeId ?? currentUser.AccountId;

        // 3. Saat disetujui, pastikan jadwal baru tidak bentrok dengan jadwal interviewer lain
        if (command.Decision == RescheduleStatus.Approved)
        {
            if (request.ProposedStart <= now || request.ProposedEnd <= request.ProposedStart)
                return Result<RescheduleRequestDto>.Failure(
                    Error.BusinessRule("Usulan jadwal sudah lewat atau rentang waktunya tidak valid. Ajukan jadwal baru."));

            if (!WEBAPI.Features.Portal.CandidateActionPolicy.IsActive(session.ApplicationStage) ||
                session.Status == InterviewSessionStatus.Rescheduled ||
                session.ScheduledStart <= now ||
                session.SessionInterviews.Any(si => si.Status == SessionInterviewStatus.Completed))
                return Result<RescheduleRequestDto>.Failure(
                    Error.BusinessRule("Reschedule hanya dapat disetujui sebelum interview dimulai dan selama tahap masih aktif."));

            var interviewerEmployeeIds = session.SessionInterviews
                .Where(si => si.Status != SessionInterviewStatus.Cancelled)
                .Select(si => si.StageEmployee.EmployeeId)
                .ToList();

            var conflictingSessions = await db.SessionInterviews
                .Include(si => si.InterviewSession)
                .Include(si => si.StageEmployee)
                    .ThenInclude(se => se.Employee)
                .Where(si =>
                    si.InterviewSessionId != session.Id &&
                    interviewerEmployeeIds.Contains(si.StageEmployee.EmployeeId) &&
                    si.Status != SessionInterviewStatus.Cancelled &&
                    si.InterviewSession.Status != InterviewSessionStatus.Rescheduled &&
                    si.InterviewSession.ScheduledStart < request.ProposedEnd &&
                    si.InterviewSession.ScheduledEnd > request.ProposedStart)
                .ToListAsync(ct);

            if (conflictingSessions.Count > 0)
            {
                var conflict = conflictingSessions[0];
                return Result<RescheduleRequestDto>.Failure(
                    Error.Conflict(
                        $"Pewawancara '{conflict.StageEmployee.Employee.FullName}' memiliki jadwal wawancara lain ('{conflict.InterviewSession.SessionName}') yang bertabrakan pada rentang waktu {conflict.InterviewSession.ScheduledStart:dd MMM yyyy HH:mm} - {conflict.InterviewSession.ScheduledEnd:HH:mm}."));
            }
        }

        // 4. Update status permohonan reschedule
        request.Status = command.Decision;
        request.ReviewedBy = reviewerId;
        request.ReviewedAt = now;
        request.ReviewNotes = command.ReviewNotes?.Trim();
        request.UpdatedAt = now;

        // 5. Jika disetujui, update sesi yang sama agar seluruh interviewer otomatis memakai jadwal baru
        if (command.Decision == RescheduleStatus.Approved)
        {
            session.ScheduledStart = request.ProposedStart;
            session.ScheduledEnd = request.ProposedEnd;
            session.Status = InterviewSessionStatus.PendingConfirmation;
            session.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        // 6. Kirim email notifikasi hasil keputusan reschedule
        await SendDecisionEmailsAsync(request, session, ct);

        return Result<RescheduleRequestDto>.Success(request.ToDto(session));
    }

    private async Task SendDecisionEmailsAsync(
        RescheduleRequest request,
        InterviewSession session,
        CancellationToken ct)
    {
        var stageName = session.ApplicationStage?.BatchStage?.StageName ?? "Wawancara";

        if (request.Status != RescheduleStatus.Approved)
        {
            try
            {
                var content = EmailTemplates.RescheduleResult(
                    session.SessionName,
                    stageName,
                    "DITOLAK",
                    $"Permohonan reschedule Anda DITOLAK oleh HR. Jadwal wawancara semula ({session.ScheduledStart:dd MMM yyyy HH:mm} - {session.ScheduledEnd:HH:mm} WIB) tetap berlaku.",
                    request.ReviewNotes);

                await emailService.SendEmailAsync(
                    request.RequesterEmail,
                    content,
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

            return;
        }

        var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;
        var recipients = new Dictionary<string, (string Name, string ActionUrl)>(StringComparer.OrdinalIgnoreCase);

        foreach (var sessionInterview in session.SessionInterviews)
        {
            var employee = sessionInterview.StageEmployee?.Employee;
            if (employee is null || string.IsNullOrWhiteSpace(employee.Email))
                continue;

            recipients[employee.Email] = (employee.FullName, frontendUrl);
        }

        if (candidate is not null && !string.IsNullOrWhiteSpace(candidate.Email))
        {
            recipients[candidate.Email] = (
                candidate.FullName,
                EmailTemplates.GetCandidatePortalUrl(configuration, candidate.Id));
        }

        foreach (var recipient in recipients)
        {
            try
            {
                var isCandidate = string.Equals(recipient.Key, candidate?.Email, StringComparison.OrdinalIgnoreCase);
                var content = EmailTemplates.InterviewRescheduled(
                    recipient.Value.Name,
                    session.SessionName,
                    stageName,
                    session.ScheduledStart,
                    session.ScheduledEnd,
                    session.IsOnline,
                    session.MeetingUrl,
                    session.Location,
                    recipient.Value.ActionUrl,
                    isCandidate && !request.RequestedByCandidate ? null : request.ReviewNotes);

                await emailService.SendEmailAsync(
                    recipient.Key,
                    content,
                    EmailType.RescheduleResult,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Gagal mengirimkan email jadwal baru reschedule untuk request ID {RequestId} ke {Recipient}",
                    request.Id,
                    recipient.Key);
            }
        }
    }
}
