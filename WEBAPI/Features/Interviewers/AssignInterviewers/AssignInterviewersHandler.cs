using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.Interviewers.AssignInterviewers;

public class AssignInterviewersHandler(
    RmsDbContext db,
    IEmailService emailService) : IRequestHandler<AssignInterviewersCommand, Result<AssignInterviewersResponse>>
{
    public async Task<Result<AssignInterviewersResponse>> Handle(
        AssignInterviewersCommand command,
        CancellationToken ct)
    {
        // 1. Ambil BatchStage beserta relasinya
        var stage = await db.BatchStages
            .Include(s => s.RecruitmentBatch)
            .Include(s => s.StageType)
            .Include(s => s.StageEmployees)
                .ThenInclude(se => se.Employee)
                    .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(s => s.Id == command.StageId, ct);

        if (stage is null)
            return Result<AssignInterviewersResponse>.Failure(
                Error.NotFound($"Tahapan dengan ID '{command.StageId}' tidak ditemukan."));

        if (stage.RecruitmentBatchId != command.BatchId)
            return Result<AssignInterviewersResponse>.Failure(
                Error.Conflict($"Tahapan '{stage.StageName}' tidak terdaftar pada Batch yang ditentukan."));

        // 2. Validasi apakah tahapan adalah tipe Interview
        if (stage.StageType.StageCategory != StageCategory.Interview)
            return Result<AssignInterviewersResponse>.Failure(
                Error.BusinessRule($"Tahapan '{stage.StageName}' bukan merupakan tahapan Wawancara (Kategori: {stage.StageType.StageCategory}). Penugasan pewawancara hanya berlaku untuk tahapan Wawancara."));

        var assignedDtos = new List<AssignedInterviewerDto>();

        // 3. Proses penugasan per pewawancara
        foreach (var employeeId in command.InterviewerEmployeeIds.Distinct())
        {
            var employee = await db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

            if (employee is null)
                return Result<AssignInterviewersResponse>.Failure(
                    Error.NotFound($"Karyawan dengan ID '{employeeId}' tidak ditemukan."));

            var existingAssignment = stage.StageEmployees
                .FirstOrDefault(se => se.EmployeeId == employeeId);

            if (existingAssignment is not null)
            {
                // Jika sudah ada sebelumnya (misal Declined lalu ditugaskan kembali, atau masih Pending)
                if (existingAssignment.Status == StageEmployeeStatus.Declined)
                {
                    existingAssignment.Status = StageEmployeeStatus.Pending;
                    existingAssignment.Note = null;
                    existingAssignment.UpdatedAt = DateTime.UtcNow;

                    await emailService.SendEmailAsync(
                        employee.Email,
                        $"Undangan Penugasan Pewawancara - {stage.StageName}",
                        $"Halo {employee.FullName},\n\nAnda kembali ditugaskan sebagai pewawancara pada tahapan '{stage.StageName}' untuk batch '{stage.RecruitmentBatch.Name}'. Silakan konfirmasi ketersediaan Anda di portal interviewer.",
                        EmailType.InterviewerInvitation,
                        ct);
                }

                existingAssignment.Employee = employee;
                assignedDtos.Add(existingAssignment.ToAssignedInterviewerDto());
            }
            else
            {
                var newAssignment = new StageEmployee
                {
                    Id = Guid.CreateVersion7(),
                    BatchStageId = stage.Id,
                    EmployeeId = employee.Id,
                    Status = StageEmployeeStatus.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.StageEmployees.Add(newAssignment);

                await emailService.SendEmailAsync(
                    employee.Email,
                    $"Undangan Penugasan Pewawancara - {stage.StageName}",
                    $"Halo {employee.FullName},\n\nAnda telah ditugaskan sebagai pewawancara pada tahapan '{stage.StageName}' untuk batch '{stage.RecruitmentBatch.Name}'. Silakan masuk ke sistem untuk merespons ketersediaan jadwal Anda.",
                    EmailType.InterviewerInvitation,
                    ct);

                newAssignment.Employee = employee;
                assignedDtos.Add(newAssignment.ToAssignedInterviewerDto());
            }
        }

        await db.SaveChangesAsync(ct);

        return Result<AssignInterviewersResponse>.Success(stage.ToAssignInterviewersResponse(assignedDtos));
    }
}
