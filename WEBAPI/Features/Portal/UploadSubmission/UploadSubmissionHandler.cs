using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Portal.UploadSubmission;

public class UploadSubmissionHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<UploadSubmissionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(
        UploadSubmissionCommand command,
        CancellationToken ct)
    {
        var session = await db.InterviewSessions
            .AsNoTracking()
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchStage)
                    .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(s => s.ApplicationStage)
                .ThenInclude(ast => ast.BatchApplication)
                    .ThenInclude(ba => ba.Candidate)
            .FirstOrDefaultAsync(s => s.Id == command.InterviewSessionId, ct);

        if (session is null)
        {
            return Result<SubmissionDto>.Failure(
                Error.NotFound($"Sesi wawancara dengan ID '{command.InterviewSessionId}' tidak ditemukan."));
        }

        // 1. Verifikasi kepemilikan sesi oleh kandidat (Portal security)
        var candidateId = session.ApplicationStage?.BatchApplication?.CandidateId;
        if (candidateId != command.CandidateId)
        {
            return Result<SubmissionDto>.Failure(
                Error.Forbidden("Sesi wawancara ini bukan milik kandidat yang bersangkutan."));
        }

        // 2. Validasi status sesi
        if (session.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<SubmissionDto>.Failure(
                Error.BusinessRule("Sesi wawancara ini telah dijadwalkan ulang."));
        }

        // 3. Validasi penugasan take-home
        if (!session.EndedSubmissionAt.HasValue)
        {
            return Result<SubmissionDto>.Failure(
                Error.BusinessRule("Sesi wawancara ini tidak memiliki penugasan take-home test."));
        }

        // 4. Validasi batas waktu pengumpulan
        var now = dateTimeProvider.GetUtcNow().UtcDateTime;
        if (now > session.EndedSubmissionAt.Value)
        {
            return Result<SubmissionDto>.Failure(
                Error.BusinessRule("Batas waktu pengumpulan tugas take-home test telah berakhir."));
        }

        if (!CandidateActionPolicy.CanSubmit(session, now))
            return Result<SubmissionDto>.Failure(Error.BusinessRule("Pengumpulan tugas sudah ditutup."));

        // 5. Simpan tautan berkas
        var submissionFile = command.FileSubmission.Trim();
        if (string.Equals(session.SubmissionFile, submissionFile, StringComparison.Ordinal))
            return Result<SubmissionDto>.Success(session.ToSubmissionDto());

        var isUpdate = !string.IsNullOrWhiteSpace(session.SubmissionFile);
        // Keep the assessment check and email queue in the same transaction as the link update.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var updated = await db.InterviewSessions
            .Where(item => item.Id == session.Id && item.SubmissionFile == session.SubmissionFile &&
                item.Status == session.Status && item.EndedSubmissionAt == session.EndedSubmissionAt)
            .Where(item => !item.SessionInterviews.Any(interviewer =>
                interviewer.Status == SessionInterviewStatus.Completed || interviewer.InterviewAssessment != null))
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.SubmissionFile, submissionFile)
                .SetProperty(item => item.UpdatedAt, now), ct);
        if (updated == 0)
            return Result<SubmissionDto>.Failure(Error.BusinessRule(
                "Tautan pengumpulan tidak dapat diubah setelah penilaian dikirim atau pengumpulan berubah."));

        session.SubmissionFile = submissionFile;
        session.UpdatedAt = now;

        var application = session.ApplicationStage!.BatchApplication;
        var stage = session.ApplicationStage.BatchStage;
        var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);
        var panel = await db.SessionInterviews.AsNoTracking()
            .Where(interviewer => interviewer.InterviewSessionId == session.Id &&
                interviewer.Status != SessionInterviewStatus.Cancelled)
            .Select(interviewer => new
            {
                interviewer.Id,
                interviewer.StageEmployee.Employee.FullName,
                interviewer.StageEmployee.Employee.Email
            })
            .ToListAsync(ct);

        var notifications = panel
            .Where(interviewer => !string.IsNullOrWhiteSpace(interviewer.Email))
            .DistinctBy(interviewer => interviewer.Email.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(interviewer => new BulkEmailMessage(interviewer.Email, EmailTemplates.TakeHomeSubmission(
                interviewer.FullName, application.Candidate.FullName, stage.RecruitmentBatch.Name,
                stage.StageName, isUpdate, $"{frontendUrl}/interviews/schedules/{interviewer.Id}")))
            .ToList();
        emailService.QueueBulkEmail(notifications, EmailType.TakeHomeSubmission);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Result<SubmissionDto>.Success(session.ToSubmissionDto());
    }
}

