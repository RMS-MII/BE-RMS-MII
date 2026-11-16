using WEBAPI.Entities.Enums;
using WEBAPI.Templates;

namespace WEBAPI.Services;

public interface IEmailService
{
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string body,
        EmailType type,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string recipientEmail,
        EmailContent content,
        EmailType type,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmailSendResult>> SendBulkEmailAsync(
        IReadOnlyCollection<BulkEmailMessage> messages,
        EmailType type,
        CancellationToken cancellationToken = default);

    Task SendVerificationEmailAsync(
        string recipientEmail,
        EmailContent content,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        EmailContent content,
        CancellationToken cancellationToken = default);

    void QueueBulkEmail(IReadOnlyCollection<BulkEmailMessage> messages, EmailType type);
}
