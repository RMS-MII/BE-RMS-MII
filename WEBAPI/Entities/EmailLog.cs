using WEBAPI.Entities.Enums;

namespace WEBAPI.Entities;

public class EmailLog
{
    public Guid  Id { get; set; } = Guid.NewGuid();
    public string SenderEmail { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public EmailType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public EmailDeliveryStatus Status { get; set; } = EmailDeliveryStatus.Sent;
    public DateTime AttemptedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
