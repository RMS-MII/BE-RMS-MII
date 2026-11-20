using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Interviewers.AssignInterviewers;

public class AssignInterviewersHandler(
    RmsDbContext db,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AssignInterviewersHandler> logger) : IRequestHandler<AssignInterviewersCommand, Result<AssignInterviewersResponse>>
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
                Error.NotFound($"Stage with ID '{command.StageId}' was not found."));

        if (stage.RecruitmentBatchId != command.BatchId)
            return Result<AssignInterviewersResponse>.Failure(
                Error.Conflict($"Stage '{stage.StageName}' is not registered in the specified batch."));

        if (stage.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<AssignInterviewersResponse>.Failure(
                Error.BusinessRule("Closed recruitment interviewer assignments cannot be changed."));

        // 2. Validasi apakah tahapan adalah tipe Interview
        if (stage.StageType.StageCategory != StageCategory.Interview)
            return Result<AssignInterviewersResponse>.Failure(
                Error.BusinessRule($"Stage '{stage.StageName}' is not an interview stage (Category: {stage.StageType.StageCategory}). Interviewer assignment only applies to interview stages."));

        var assignedDtos = new List<AssignedInterviewerDto>();
        var emailsToSend = new List<(string Recipient, EmailContent Content)>();
        var frontendUrl = EmailTemplates.GetFrontendUrl(configuration);

        // 3. Proses penugasan per pewawancara
        foreach (var employeeId in command.InterviewerEmployeeIds.Distinct())
        {
            var employee = await db.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

            if (employee is null)
                return Result<AssignInterviewersResponse>.Failure(
                    Error.NotFound($"Employee with ID '{employeeId}' was not found."));

            var existingAssignment = stage.StageEmployees
                .FirstOrDefault(se => se.EmployeeId == employeeId);

            if (existingAssignment is not null)
            {
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

                emailsToSend.Add((
                    employee.Email,
                    EmailTemplates.InterviewerAssignment(
                        employee.FullName,
                        stage.StageName,
                        stage.RecruitmentBatch.Name,
                        isReassignment: false,
                        frontendUrl)));

                newAssignment.Employee = employee;
                assignedDtos.Add(newAssignment.ToAssignedInterviewerDto());
            }
        }

        await db.SaveChangesAsync(ct);

        foreach (var email in emailsToSend)
        {
            try
            {
                await emailService.SendEmailAsync(
                    email.Recipient,
                    email.Content,
                    EmailType.InterviewerInvitation,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Gagal mengirim email undangan penugasan untuk stage {StageId}",
                    stage.Id);
            }
        }

        return Result<AssignInterviewersResponse>.Success(stage.ToAssignInterviewersResponse(assignedDtos));
    }
}
