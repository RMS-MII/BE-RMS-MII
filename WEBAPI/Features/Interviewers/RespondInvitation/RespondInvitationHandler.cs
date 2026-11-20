using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Interviewers.RespondInvitation;

public class RespondInvitationHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService) : IRequestHandler<RespondInvitationCommand, Result<RespondInvitationResponse>>
{
    public async Task<Result<RespondInvitationResponse>> Handle(
        RespondInvitationCommand command,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId;
        if (!employeeId.HasValue)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.Unauthorized("Your account is not linked to an employee profile."));
        }

        var assignment = await db.StageEmployees
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(se => se.Employee)
            .FirstOrDefaultAsync(se => se.Id == command.InvitationId, ct);

        if (assignment is null)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.NotFound($"Assignment invitation with ID '{command.InvitationId}' was not found."));
        }

        if (assignment.EmployeeId != employeeId.Value)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.Forbidden("You do not have permission to respond to an assignment belonging to another interviewer."));
        }

        if (assignment.BatchStage.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<RespondInvitationResponse>.Failure(
                Error.BusinessRule("A closed recruitment batch cannot be edited."));

        // Cek jika penugasan sudah memiliki jadwal sesi wawancara yang aktif
        var hasActiveSessions = await db.SessionInterviews
            .AnyAsync(si => si.StageEmployeeId == assignment.Id && si.Status != SessionInterviewStatus.Cancelled, ct);

        if (hasActiveSessions && command.Response == StageEmployeeStatus.Declined)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.BusinessRule("This assignment cannot be declined because you are already scheduled for an ongoing interview session. Contact HR to cancel the schedule."));
        }

        var now = DateTime.UtcNow;
        assignment.Status = command.Response;
        assignment.Note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim();
        assignment.UpdatedAt = now;

        // Jika menolak penugasan, kirim email notifikasi ke HR pembuat batch / HR department
        if (command.Response == StageEmployeeStatus.Declined)
        {
            var hrEmployee = await db.Employees
                .FirstOrDefaultAsync(e => e.Id == assignment.BatchStage.RecruitmentBatch.CreatedBy, ct);

            if (hrEmployee is not null && !string.IsNullOrWhiteSpace(hrEmployee.Email))
            {
                var content = EmailTemplates.InterviewerDeclined(
                    hrEmployee.FullName,
                    assignment.Employee.FullName,
                    assignment.BatchStage.StageName,
                    assignment.BatchStage.RecruitmentBatch.Name,
                    assignment.Note);

                await emailService.SendEmailAsync(
                    hrEmployee.Email,
                    content,
                    EmailType.InterviewerInvitation,
                    ct);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result<RespondInvitationResponse>.Success(assignment.ToRespondInvitationResponse(now));
    }
}
