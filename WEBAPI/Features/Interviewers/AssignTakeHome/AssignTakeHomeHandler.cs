using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Features.Portal;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Interviewers.AssignTakeHome;

public class AssignTakeHomeHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AssignTakeHomeHandler> logger) : IRequestHandler<AssignTakeHomeCommand, Result<AssignTakeHomeResponse>>
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
                    .ThenInclude(batchStage => batchStage.RecruitmentBatch)
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

        if (session.Status != InterviewSessionStatus.AcceptedByCandidate || !CandidateActionPolicy.IsActive(session.ApplicationStage))
            return Result<AssignTakeHomeResponse>.Failure(
                Error.BusinessRule("Take-home hanya dapat diberikan setelah jadwal dikonfirmasi kandidat, selama tahap masih aktif."));

        session.FileTakeHome = command.FileTakeHome.Trim();
        session.EndedSubmissionAt = command.EndedSubmissionAt;
        session.UpdatedAt = now;

        // 5. Kirim email instruksi take-home test kepada kandidat (Aturan Bisnis #9)
        var candidate = session.ApplicationStage.BatchApplication.Candidate;
        var stageName = session.ApplicationStage.BatchStage.StageName;
        var portalUrl = EmailTemplates.GetCandidatePortalUrl(configuration, candidate.Id);
        var submissionPortalLink = $"{portalUrl}/assignments?session={session.Id}";

        var content = EmailTemplates.TakeHome(
            candidate.FullName,
            stageName,
            command.TaskTitle,
            command.EndedSubmissionAt,
            command.Instructions,
            session.FileTakeHome,
            submissionPortalLink);

        await db.SaveChangesAsync(ct);

        var emailSent = false;

        try
        {
            await emailService.SendEmailAsync(
                candidate.Email,
                content,
                EmailType.TakeHome,
                ct);

            emailSent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim email instruksi take-home untuk session {SessionId}",
                session.Id);
        }

        var message = emailSent
            ? "Tugas take-home test berhasil ditetapkan dan instruksi telah dikirimkan ke email kandidat."
            : "Tugas take-home test berhasil ditetapkan, tetapi email instruksi gagal dikirim.";

        return Result<AssignTakeHomeResponse>.Success(
            session.ToAssignTakeHomeResponse(emailSent, message));
    }
}
