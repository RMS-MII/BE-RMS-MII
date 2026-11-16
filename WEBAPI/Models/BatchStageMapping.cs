using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class BatchStageMapping
{
    public static BatchStageDto ToDto(this BatchStage stage) => MapToDto<BatchStageDto>(stage);

    public static BatchStageWithInterviewersDto ToDtoWithInterviewers(this BatchStage stage)
    {
        var dto = MapToDto<BatchStageWithInterviewersDto>(stage);
        dto.Interviewers = stage.StageEmployees
            .OrderBy(se => se.Employee.FullName)
            .ThenBy(se => se.EmployeeId)
            .ToAssignedInterviewerDtoList();
        return dto;
    }

    private static T MapToDto<T>(BatchStage stage) where T : BatchStageDto, new() => new()
    {
        Id = stage.Id,
        RecruitmentBatchId = stage.RecruitmentBatchId,
        StageTypeId = stage.StageTypeId,
        StageTypeName = stage.StageType?.Name,
        StageCategory = stage.StageType?.StageCategory,
        StageName = stage.StageName,
        LifecycleStatus = stage.LifecycleStatus,
        PassingScore = stage.PassingScore,
        StartDate = stage.StartDate,
        EndDate = stage.EndDate,
        NextStageId = stage.NextStageId,
        CreatedAt = stage.CreatedAt,
        UpdatedAt = stage.UpdatedAt
    };

    public static IReadOnlyCollection<BatchStageDto> ToDtoList(this IEnumerable<BatchStage> stages)
        => stages.Select(ToDto).ToList();

    public static BatchStage ToEntity(this CreateBatchStageDto dto) => new()
    {
        RecruitmentBatchId = dto.RecruitmentBatchId,
        StageTypeId = dto.StageTypeId,
        StageName = dto.StageName.Trim(),
        PassingScore = dto.PassingScore,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        NextStageId = dto.NextStageId
    };

    public static void ApplyTo(this UpdateBatchStageDto dto, BatchStage stage)
    {
        stage.StageTypeId = dto.StageTypeId;
        stage.StageName = dto.StageName.Trim();
        stage.PassingScore = dto.PassingScore;
        stage.StartDate = dto.StartDate;
        stage.EndDate = dto.EndDate;
        stage.NextStageId = dto.NextStageId;
    }
}
