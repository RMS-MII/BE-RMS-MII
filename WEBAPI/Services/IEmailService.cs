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
}
