using WEBAPI.Features.Batches;
using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class RecruitmentBatchMapping
{
    public static RecruitmentBatchDto ToDto(this RecruitmentBatch batch) => new()
    {
        Id = batch.Id,
        DepartmentId = batch.DepartmentId,
        DepartmentName = batch.Department?.Name,
        BatchCode = batch.BatchCode,
        Name = batch.Name,
        Description = batch.Description,
        StartDate = batch.StartDate,
        EndDate = batch.EndDate,
        Status = batch.Status,
        CreatedBy = batch.CreatedBy,
        CreatedAt = batch.CreatedAt,
        UpdatedAt = batch.UpdatedAt,
        BatchStages = BatchStageSequence.GetOrderedStages(batch.BatchStages.ToList()).ToDtoList()
    };

    public static IReadOnlyCollection<RecruitmentBatchDto> ToDtoList(this IEnumerable<RecruitmentBatch> batches)
        => batches.Select(ToDto).ToList();

    public static RecruitmentBatch ToEntity(this CreateRecruitmentBatchDto dto) => new()
    {
        DepartmentId = dto.DepartmentId,
        BatchCode = dto.BatchCode.Trim(),
        Name = dto.Name.Trim(),
        Description = dto.Description.Trim(),
        StartDate = dto.StartDate,
        EndDate = dto.EndDate
    };

    public static void ApplyTo(this UpdateRecruitmentBatchDto dto, RecruitmentBatch batch)
    {
        batch.DepartmentId = dto.DepartmentId;
        batch.BatchCode = dto.BatchCode.Trim();
        batch.Name = dto.Name.Trim();
        batch.Description = dto.Description.Trim();
        batch.StartDate = dto.StartDate;
        batch.EndDate = dto.EndDate;
        batch.Status = dto.Status;
    }
}
