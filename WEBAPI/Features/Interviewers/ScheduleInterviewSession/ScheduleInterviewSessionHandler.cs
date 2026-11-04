using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Interviewers.ScheduleInterviewSession;

public class ScheduleInterviewSessionHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IEmailService emailService) : IRequestHandler<ScheduleInterviewSessionCommand, Result<InterviewSessionDto>>
{
    public async Task<Result<InterviewSessionDto>> Handle(
        ScheduleInterviewSessionCommand command,
        CancellationToken ct)
    {
        // 1. Ambil ApplicationStage beserta relasinya
        var appStage = await db.ApplicationStages
            .Include(a => a.BatchStage)
                .ThenInclude(bs => bs.StageType)
            .Include(a => a.BatchStage)
                .ThenInclude(bs => bs.RecruitmentBatch)
            .Include(a => a.BatchStage)
                .ThenInclude(bs => bs.StageEmployees)
                    .ThenInclude(se => se.Employee)
                        .ThenInclude(e => e.Department)
            .Include(a => a.BatchApplication)
                .ThenInclude(ba => ba.Candidate)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicationStageId, ct);

        if (appStage is null)
        {
            return Result<InterviewSessionDto>.Failure(
                Error.NotFound($"ApplicationStage dengan ID '{command.ApplicationStageId}' tidak ditemukan."));
        }

        // 2. Validasi kategori tahapan harus Wawancara
        if (appStage.BatchStage.StageType.StageCategory != StageCategory.Interview)
        {
            return Result<InterviewSessionDto>.Failure(
                Error.BusinessRule($"Tahapan '{appStage.BatchStage.StageName}' bertipe {appStage.BatchStage.StageType.StageCategory}. Penjadwalan sesi wawancara hanya berlaku untuk tahapan bertipe Wawancara."));
        }

        // 3. Validasi status ApplicationStage
        if (appStage.Status != ApplicationStageStatus.InProgress)
        {
            return Result<InterviewSessionDto>.Failure(
                Error.BusinessRule($"Tidak dapat menjadwalkan wawancara karena status tahapan kandidat saat ini adalah '{appStage.Status.ToDisplayName()}'."));
        }

        // 4. Validasi ketersediaan dan status pewawancara yang dipilih
        var distinctEmployeeIds = command.InterviewerEmployeeIds.Distinct().ToList();
        var matchedStageEmployees = new List<StageEmployee>();

        foreach (var employeeId in distinctEmployeeIds)
        {
            var stageEmployee = appStage.BatchStage.StageEmployees
                .FirstOrDefault(se => se.EmployeeId == employeeId && se.IsActive);

            if (stageEmployee is null)
            {
                var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct);
                var empName = employee?.FullName ?? employeeId.ToString();
                return Result<InterviewSessionDto>.Failure(
                    Error.NotFound($"Pewawancara '{empName}' belum ditugaskan pada tahapan '{appStage.BatchStage.StageName}'."));
            }

            if (stageEmployee.Status != StageEmployeeStatus.Accepted)
            {
                return Result<InterviewSessionDto>.Failure(
                    Error.BusinessRule($"Pewawancara '{stageEmployee.Employee.FullName}' belum menyetujui penugasan pada tahapan ini (Status saat ini: {stageEmployee.Status.ToDisplayName()}). Sesi hanya dapat dijadwalkan dengan pewawancara yang sudah berstatus 'Bersedia' (Accepted)."));
            }

            matchedStageEmployees.Add(stageEmployee);
        }

        // 5. Cek konflik jadwal sesi lain untuk para pewawancara terpilih
        var selectedStageEmployeeIds = matchedStageEmployees.Select(se => se.Id).ToList();
        var conflictingSessions = await db.SessionInterviews
            .Include(si => si.InterviewSession)
            .Include(si => si.StageEmployee)
                .ThenInclude(se => se.Employee)
            .Where(si => selectedStageEmployeeIds.Contains(si.StageEmployeeId)
                      && si.Status != SessionInterviewStatus.Cancelled
                      && si.InterviewSession.Status != InterviewSessionStatus.Rescheduled
                      && si.InterviewSession.ScheduledStart < command.ScheduledEnd
                      && si.InterviewSession.ScheduledEnd > command.ScheduledStart)
            .ToListAsync(ct);

        if (conflictingSessions.Count > 0)
        {
            var conflict = conflictingSessions.First();
            return Result<InterviewSessionDto>.Failure(
                Error.Conflict($"Pewawancara '{conflict.StageEmployee.Employee.FullName}' memiliki jadwal wawancara lain ('{conflict.InterviewSession.SessionName}') yang bertabrakan pada rentang waktu {conflict.InterviewSession.ScheduledStart:dd MMM yyyy HH:mm} - {conflict.InterviewSession.ScheduledEnd:HH:mm}."));
        }

        var now = DateTime.UtcNow;

        // 6. Buat entitas InterviewSession
        var session = new InterviewSession
        {
            Id = Guid.CreateVersion7(),
            ApplicationStageId = appStage.Id,
            SessionName = command.SessionName.Trim(),
            ScheduledStart = command.ScheduledStart,
            ScheduledEnd = command.ScheduledEnd,
            IsOnline = command.IsOnline,
            Location = command.IsOnline ? null : command.Location?.Trim(),
            MeetingUrl = command.IsOnline ? command.MeetingUrl?.Trim() : null,
            Status = InterviewSessionStatus.PendingConfirmation,
            CreatedBy = currentUser.EmployeeId ?? currentUser.AccountId ?? Guid.Empty,
            CreatedAt = now,
            ApplicationStage = appStage
        };

        // 7. Buat SessionInterview untuk setiap pewawancara terpilih & kirim email undangan ke pewawancara
        foreach (var se in matchedStageEmployees)
        {
            var sessionInterview = new SessionInterview
            {
                Id = Guid.CreateVersion7(),
                InterviewSessionId = session.Id,
                StageEmployeeId = se.Id,
                Status = SessionInterviewStatus.InProgress,
                CreatedAt = now,
                StageEmployee = se
            };
            session.SessionInterviews.Add(sessionInterview);

            await emailService.SendEmailAsync(
                se.Employee.Email,
                $"Jadwal Sesi Wawancara - {session.SessionName}",
                $"Halo {se.Employee.FullName},\n\nAnda telah dijadwalkan oleh HR untuk sesi wawancara:\n- Sesi: {session.SessionName}\n- Kandidat: {appStage.BatchApplication.Candidate.FullName}\n- Tahapan: {appStage.BatchStage.StageName}\n- Waktu: {session.ScheduledStart:dd MMM yyyy HH:mm} - {session.ScheduledEnd:HH:mm} WIB\n- Moda: {(session.IsOnline ? $"Daring ({session.MeetingUrl})" : $"Tatap Muka di {session.Location}")}\n\nSilakan periksa dan konfirmasi kehadiran Anda di portal wawancara.",
                EmailType.InterviewerInvitation,
                ct);
        }

        db.InterviewSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return Result<InterviewSessionDto>.Success(session.ToInterviewSessionDto());
    }
}
