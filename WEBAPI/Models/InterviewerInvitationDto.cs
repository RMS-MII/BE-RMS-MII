using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public class InterviewerInvitationDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string StageTypeName { get; set; } = string.Empty;
    public DateTime StageStartDate { get; set; }
    public DateTime StageEndDate { get; set; }
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public string? Note { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public record RespondInvitationRequest(
    StageEmployeeStatus Response,
    string? Note = null);

public class RespondInvitationResponse
{
    public Guid InvitationId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public StageEmployeeStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public string? Note { get; set; }
    public DateTime RespondedAt { get; set; }
}

