using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

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
                Error.Unauthorized("Akun Anda tidak terhubung dengan profil karyawan."));
        }

        var assignment = await db.StageEmployees
            .Include(se => se.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(se => se.Employee)
            .FirstOrDefaultAsync(se => se.Id == command.InvitationId, ct);

        if (assignment is null)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.NotFound($"Undangan penugasan dengan ID '{command.InvitationId}' tidak ditemukan."));
        }

        if (assignment.EmployeeId != employeeId.Value)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.Forbidden("Anda tidak memiliki izin untuk merespons penugasan milik pewawancara lain."));
        }

        // Cek jika penugasan sudah memiliki jadwal sesi wawancara yang aktif
        var hasActiveSessions = await db.SessionInterviews
            .AnyAsync(si => si.StageEmployeeId == assignment.Id && si.Status != SessionInterviewStatus.Cancelled, ct);

        if (hasActiveSessions && command.Response == StageEmployeeStatus.Declined)
        {
            return Result<RespondInvitationResponse>.Failure(
                Error.BusinessRule("Penugasan ini tidak dapat ditolak karena Anda sudah dijadwalkan pada sesi wawancara yang sedang berjalan. Hubungi HR untuk pembatalan jadwal."));
        }

        var now = DateTime.UtcNow;
        assignment.Status = command.Response;
        assignment.Note = command.Response == StageEmployeeStatus.Declined ? command.Note?.Trim() : null;
        assignment.UpdatedAt = now;

        // Jika menolak penugasan, kirim email notifikasi ke HR pembuat batch / HR department
        if (command.Response == StageEmployeeStatus.Declined)
        {
            var hrEmployee = await db.Employees
                .FirstOrDefaultAsync(e => e.Id == assignment.BatchStage.RecruitmentBatch.CreatedBy, ct);

            if (hrEmployee is not null && !string.IsNullOrWhiteSpace(hrEmployee.Email))
            {
                await emailService.SendEmailAsync(
                    hrEmployee.Email,
                    $"Penolakan Penugasan Pewawancara - {assignment.Employee.FullName}",
                    $"Halo {hrEmployee.FullName},\n\nPewawancara {assignment.Employee.FullName} telah menolak penugasan pada tahapan '{assignment.BatchStage.StageName}' (Batch: {assignment.BatchStage.RecruitmentBatch.Name}).\n\nAlasan penolakan: {assignment.Note}\n\nSilakan tentukan pewawancara pengganti pada sistem RMS.",
                    EmailType.InterviewerInvitation,
                    ct);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result<RespondInvitationResponse>.Success(assignment.ToRespondInvitationResponse(now));
    }
}
