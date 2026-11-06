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

        string batchCode;
        if (!string.IsNullOrWhiteSpace(request.BatchCode))
        {
            batchCode = request.BatchCode.Trim();

            var codeExists = await db.RecruitmentBatches
                .AnyAsync(b => b.BatchCode == batchCode, ct);

            if (codeExists)
                return Result<RecruitmentBatchDto>.Failure(
                    Error.Conflict($"Batch code '{batchCode}' already exists."));
        }
        else
        {
            var year = request.StartDate != default ? request.StartDate.Year : dateTimeProvider.GetUtcNow().UtcDateTime.Year;
            var deptCode = string.IsNullOrWhiteSpace(department.Code) ? "GEN" : department.Code.Trim().ToUpperInvariant();
            var prefix = $"BATCH-{deptCode}-{year}-";

            var existingCodes = await db.RecruitmentBatches
                .AsNoTracking()
                .Where(b => b.BatchCode.StartsWith(prefix))
                .Select(b => b.BatchCode)
                .ToListAsync(ct);

            var maxSeq = 0;
            foreach (var code in existingCodes)
            {
                var suffix = code[prefix.Length..];
                if (int.TryParse(suffix, out var seq) && seq > maxSeq)
                {
                    maxSeq = seq;
                }
            }

            batchCode = $"{prefix}{(maxSeq + 1):D3}";
        }

        var batch = new RecruitmentBatch
        {
            Id = Guid.CreateVersion7(),
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
            logger.LogWarning("Create batch gagal: kode batch {BatchCode} sudah dipakai", batchCode);
            return Result<RecruitmentBatchDto>.Failure(
                Error.Conflict("Batch code already exists."));
        }

        var dto = batch.ToDto();
        dto.DepartmentName = department.Name;

        return Result<RecruitmentBatchDto>.Success(dto);
    }
}
