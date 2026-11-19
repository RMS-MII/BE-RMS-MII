using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class BatchApplicationMapping
{
    public static BatchApplicationDto ToDto(this BatchApplication application, Guid? stageId = null)
        => Map(application, application.Candidate, stageId);

    public static BatchApplicationDto ToDto(this BatchApplication application, Candidate candidate)
        => Map(application, candidate);

    public static IReadOnlyCollection<BatchApplicationDto> ToDtoList(
        this IEnumerable<BatchApplication> applications, Guid? stageId = null)
        => applications.Select(application => Map(application, application.Candidate, stageId)).ToList();

    private static BatchApplicationDto Map(
        BatchApplication application, Candidate? candidate, Guid? stageId = null)
    {
        var currentStage = application.ApplicationStages
            .Where(stage => !stageId.HasValue || stage.BatchStageId == stageId.Value)
            .OrderBy(stage => stage.BatchStage?.StartDate ?? stage.StartedAt)
            .ThenBy(stage => stage.BatchStage?.CreatedAt ?? stage.CreatedAt)
            .ThenBy(stage => stage.StartedAt)
            .ToList()
            .LastOrDefault();

        // Standalone candidate yang belum di-start belum punya ApplicationStage.
        // Saat daftar difilter oleh stage, tampilkan identitas stage dari konfigurasi batch.
        var unstartedStage = currentStage is null && application.ApplicationStages.Count == 0 &&
                             stageId.HasValue
            ? application.RecruitmentBatch?.BatchStages
                .FirstOrDefault(stage => stage.Id == stageId.Value)
            : null;

        return new BatchApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            RecruitmentBatchId = application.RecruitmentBatchId,
            IsStandalone = application.IsStandalone,
            ExpectedSalary = application.ExpectedSalary,
            CodeApplication = application.CodeApplication,
            AppliedAt = application.AppliedAt,
            FinalResultStatus = application.FinalResultStatus,
            Status = application.Status,
            Notes = application.Notes,
            OfferingLetterStatus = application.OfferingLetterStatus,
            FileOfferingLetter = application.FileOfferingLetter,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            CurrentStageId = currentStage?.BatchStageId ?? unstartedStage?.Id,
            CurrentStageName = currentStage?.BatchStage?.StageName ?? unstartedStage?.StageName,
            CurrentStageStatus = currentStage?.Status,
            Candidate = candidate?.ToDto()
        };
    }
}
