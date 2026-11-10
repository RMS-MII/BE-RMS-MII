using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public static class BatchApplicationMapping
{
    public static BatchApplicationDto ToDto(this BatchApplication application)
        => Map(application, application.Candidate);

    public static BatchApplicationDto ToDto(this BatchApplication application, Candidate candidate)
        => Map(application, candidate);

    public static IReadOnlyCollection<BatchApplicationDto> ToDtoList(
        this IEnumerable<BatchApplication> applications)
        => applications.Select(ToDto).ToList();

    private static BatchApplicationDto Map(BatchApplication application, Candidate? candidate)
    {
        var currentStage = application.ApplicationStages
            .OrderBy(stage => stage.BatchStage?.StartDate ?? stage.StartedAt)
            .ThenBy(stage => stage.BatchStage?.CreatedAt ?? stage.CreatedAt)
            .ThenBy(stage => stage.StartedAt)
            .ToList()
            .LastOrDefault();

        return new BatchApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            RecruitmentBatchId = application.RecruitmentBatchId,
            CodeApplication = application.CodeApplication,
            AppliedAt = application.AppliedAt,
            FinalResultStatus = application.FinalResultStatus,
            Status = application.Status,
            Notes = application.Notes,
            OfferingLetterStatus = application.OfferingLetterStatus,
            FileOfferingLetter = application.FileOfferingLetter,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            CurrentStageId = currentStage?.BatchStageId,
            CurrentStageName = currentStage?.BatchStage?.StageName,
            CurrentStageStatus = currentStage?.Status,
            Candidate = candidate?.ToDto()
        };
    }
}
