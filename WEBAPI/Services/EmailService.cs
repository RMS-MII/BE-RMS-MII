using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Templates;

namespace WEBAPI.Services;

public class EmailService(RmsDbContext dbContext, IConfiguration configuration) : IEmailService
{
    private readonly EmailSettings _settings = EmailSettings.FromConfiguration(configuration);

    public Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string body,
        EmailType type,
        CancellationToken cancellationToken = default)
        => SendEmailAsync(
            recipientEmail,
            new EmailContent(subject, body),
            type,
            cancellationToken);

    public async Task SendEmailAsync(
        string recipientEmail,
        EmailContent content,
        EmailType type,
        CancellationToken cancellationToken = default)
    {
        var recipient = recipientEmail.Trim();
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient email must not be empty.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(content.Subject))
            throw new ArgumentException("Email subject must not be empty.", nameof(content));

        var attemptedAt = DateTime.UtcNow;
        var log = new EmailLog
        {
            SenderEmail = _settings.SenderEmail,
            RecipientEmail = recipient,
            Subject = content.Subject,
            Body = content.TextBody,
            Type = type,
            Status = EmailDeliveryStatus.Sent,
            AttemptedAt = attemptedAt,
            SentAt = null
        };

        try
        {
            var message = new MimeMessage
            {
                MessageId = MimeUtils.GenerateMessageId(),
                Subject = content.Subject,
                Body = BuildMessageBody(content)
            };

            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(recipient));

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.Security,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
                await client.AuthenticateAsync(
                    _settings.UserName,
                    _settings.Password,
                    cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            log.SentAt = DateTime.UtcNow;
            Log.Information("Sent email [{Type}] to {Recipient}: {Subject}", type, recipient, content.Subject);
        }
        catch (Exception exception)
        {
            log.Status = EmailDeliveryStatus.Failed;
            log.ErrorMessage = Truncate(exception.GetBaseException().Message, 1000);
            await SaveLogSafelyAsync(log);
            Log.Error(exception, "Failed to send email [{Type}] to {Recipient}: {Subject}", type, recipient, content.Subject);
            throw;
        }

        await SaveLogSafelyAsync(log);
    }

    private async Task SaveLogSafelyAsync(EmailLog emailLog)
    {
        try
        {
            dbContext.EmailLogs.Add(emailLog);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            dbContext.Entry(emailLog).State = EntityState.Detached;
            Log.Error(exception, "Failed to persist email delivery log {EmailLogId}", emailLog.Id);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static MimeEntity BuildMessageBody(EmailContent content)
    {
        var bodyBuilder = new BodyBuilder
        {
            TextBody = content.TextBody
        };

        if (!string.IsNullOrWhiteSpace(content.HtmlBody))
            bodyBuilder.HtmlBody = content.HtmlBody;

        return bodyBuilder.ToMessageBody();
    }

    private sealed class EmailSettings
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 2525;
        public SecureSocketOptions Security { get; init; } = SecureSocketOptions.None;
        public string SenderName { get; init; } = "RMS";
        public string SenderEmail { get; init; } = "no-reply@rms.local";
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public static EmailSettings FromConfiguration(IConfiguration configuration)
        {
            var portValue = configuration["MailSettings:Port"];
            var port = int.TryParse(portValue, out var parsedPort) && parsedPort > 0
                ? parsedPort
                : 2525;

            return new EmailSettings
            {
                Host = configuration["MailSettings:Host"]?.Trim() is { Length: > 0 } host
                    ? host
                    : "localhost",
                Port = port,
                Security = ParseSecurity(configuration["MailSettings:Security"]),
                SenderName = configuration["MailSettings:SenderName"]?.Trim() is { Length: > 0 } senderName
                    ? senderName
                    : "RMS",
                SenderEmail = configuration["MailSettings:SenderEmail"]?.Trim() is { Length: > 0 } senderEmail
                    ? senderEmail
                    : "no-reply@rms.local",
                UserName = configuration["MailSettings:UserName"]?.Trim() ?? string.Empty,
                Password = configuration["MailSettings:Password"] ?? string.Empty
            };
        }

        private static SecureSocketOptions ParseSecurity(string? value) =>
            value?.Trim().ToLowerInvariant() switch
            {
                "auto" => SecureSocketOptions.Auto,
                "ssl" or "sslonconnect" => SecureSocketOptions.SslOnConnect,
                "starttls" or "starttlsrequired" => SecureSocketOptions.StartTls,
                "starttlswhenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
                _ => SecureSocketOptions.None
            };
    }
}
