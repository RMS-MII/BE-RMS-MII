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
                Error.Forbidden("Your account is not linked to an interviewer employee profile."));
        }

        // 1. Ambil InterviewSession beserta relasi ApplicationStage, Candidate, SessionInterviews
        var session = await db.InterviewSessions
            .AsNoTracking()
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
                Error.NotFound($"Interview session with ID '{command.InterviewSessionId}' was not found."));
        }

        // 2. Validasi pewawancara ditugaskan pada sesi ini
        var assignment = session.SessionInterviews.FirstOrDefault(si =>
            si.StageEmployee.EmployeeId == currentEmployeeId.Value && si.Status != SessionInterviewStatus.Cancelled);

        if (assignment is null)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.Forbidden("You do not have permission to assign a take-home task to this interview session."));
        }

        // 3. Validasi status sesi
        if (session.Status == InterviewSessionStatus.Rescheduled)
        {
            return Result<AssignTakeHomeResponse>.Failure(
                Error.BusinessRule("A take-home task cannot be assigned because this session has been rescheduled."));
        }

        var now = DateTime.UtcNow;

        if (session.Status != InterviewSessionStatus.AcceptedByCandidate || !CandidateActionPolicy.IsActive(session.ApplicationStage))
            return Result<AssignTakeHomeResponse>.Failure(
                Error.BusinessRule("A take-home task can only be assigned after the candidate confirms the schedule and while the stage is still active."));

        var fileTakeHome = command.FileTakeHome.Trim();
        var updated = await db.InterviewSessions
            .Where(item => item.Id == session.Id && item.Status == InterviewSessionStatus.AcceptedByCandidate)
            .Where(item => item.SessionInterviews.Any(interviewer =>
                interviewer.Id == assignment.Id &&
                interviewer.Status != SessionInterviewStatus.Cancelled &&
                interviewer.Status != SessionInterviewStatus.Completed &&
                interviewer.InterviewAssessment == null))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.FileTakeHome, fileTakeHome)
                .SetProperty(item => item.EndedSubmissionAt, command.EndedSubmissionAt)
                .SetProperty(item => item.UpdatedAt, now), ct);

        if (updated == 0)
            return Result<AssignTakeHomeResponse>.Failure(
                Error.BusinessRule("A take-home task cannot be changed after the assessment is submitted or the session is changed."));

        session.FileTakeHome = fileTakeHome;
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
