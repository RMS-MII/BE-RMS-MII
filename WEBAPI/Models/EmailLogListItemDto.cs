using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

public sealed class EmailLogListItemDto
{
    public Guid Id { get; init; }
    public string SenderEmail { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public EmailType Type { get; init; }
    public string Subject { get; init; } = string.Empty;
    public EmailDeliveryStatus Status { get; init; }
    public DateTime AttemptedAt { get; init; }
    public DateTime? SentAt { get; init; }
    public string? ErrorMessage { get; init; }
    public bool CanRetry { get; init; }
}
