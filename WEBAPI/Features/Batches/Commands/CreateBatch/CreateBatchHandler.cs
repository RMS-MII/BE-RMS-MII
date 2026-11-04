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

namespace WEBAPI.Features.Batches.Commands.CreateBatch;

public sealed class CreateBatchHandler(
    RmsDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    ILogger<CreateBatchHandler> logger) : IRequestHandler<CreateBatchCommand, Result<RecruitmentBatchDto>>
{
    public async Task<Result<RecruitmentBatchDto>> Handle(CreateBatchCommand request, CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId;
        if (employeeId is null)
            return Result<RecruitmentBatchDto>.Failure(
                Error.Unauthorized("Authentication is required."));

        var department = await db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, ct);

        if (department is null)
            return Result<RecruitmentBatchDto>.Failure(
                Error.NotFound("Department was not found."));

        var batchCode = request.BatchCode.Trim();

        var codeExists = await db.RecruitmentBatches
            .AnyAsync(b => b.BatchCode == batchCode, ct);

        if (codeExists)
            return Result<RecruitmentBatchDto>.Failure(
                Error.Conflict("Batch code already exists."));

        var batch = new RecruitmentBatch
        {
            Id = Guid.NewGuid(),
            DepartmentId = department.Id,
            BatchCode = batchCode,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = BatchStatus.Draft,
            CreatedBy = employeeId.Value,
            CreatedAt = dateTimeProvider.GetUtcNow().UtcDateTime
        };

        db.RecruitmentBatches.Add(batch);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            // Dua request bisa lolos pengecekan di atas bersamaan; unique index yang jadi penentu akhir.
            logger.LogWarning("Create batch gagal: kode batch {BatchCode} sudah dipakai", batchCode);
            return Result<RecruitmentBatchDto>.Failure(
                Error.Conflict("Batch code already exists."));
        }

        var dto = batch.ToDto();
        dto.DepartmentName = department.Name;

        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
