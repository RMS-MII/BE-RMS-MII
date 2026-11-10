using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class BulkStartNextStageDto
{
    public Guid BatchId { get; set; }
    public Guid CurrentStageId { get; set; }
    public string CurrentStageName { get; set; } = string.Empty;
    public Guid? NextStageId { get; set; }
    public string? NextStageName { get; set; }
    public int TotalCandidates { get; set; }
    public int EvaluatedCandidates { get; set; }
    public int PassedCandidates { get; set; }
    public int StartedCount { get; set; }
    public int SkippedNotPassedCount { get; set; }
    public int SkippedAlreadyStartedCount { get; set; }
    public int SkippedFinalCount { get; set; }
    public int EmailSentCount { get; set; }
    public int EmailFailedCount { get; set; }
    public IReadOnlyCollection<BulkStartNextStageCandidateDto> Candidates { get; set; } = [];
}

public class BulkStartNextStageCandidateDto
{
    public Guid ApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public Guid? ApplicationStageId { get; set; }
    public ApplicationStageStatus StageStatus { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool EmailSent { get; set; }
}
