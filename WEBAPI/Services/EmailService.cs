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
        var recipient = NormalizeRecipient(recipientEmail);
        ValidateContent(content);

        var message = BuildMessage(recipient, content);

        using var client = new SmtpClient();
        await ConnectAsync(client, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

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
        Log.Information("Sent email [{Type}] to {Recipient}: {Subject}", type, recipient, content.Subject);

        dbContext.EmailLogs.Add(CreateLog(recipient, content, type));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailSendResult>> SendBulkEmailAsync(
        IReadOnlyCollection<BulkEmailMessage> messages,
        EmailType type,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return [];

        var results = new List<EmailSendResult>(messages.Count);
        var logs = new List<EmailLog>(messages.Count);
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(recipient));

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.Security,
                cancellationToken);
        using var client = new SmtpClient();

        try
        {
            await ConnectAsync(client, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to connect to SMTP server for bulk email [{Type}]", type);

            return messages
                .Select(message => new EmailSendResult(
                    message.RecipientEmail,
                    false,
                    ex.Message))
                .ToList();
        }

        foreach (var item in messages)
        {
            try
            {
                var recipient = NormalizeRecipient(item.RecipientEmail);
                ValidateContent(item.Content);

                var message = BuildMessage(recipient, item.Content);
                await client.SendAsync(message, cancellationToken);

                Log.Information(
                    "Sent email [{Type}] to {Recipient}: {Subject}",
                    type,
                    recipient,
                    item.Content.Subject);

                logs.Add(CreateLog(recipient, item.Content, type));
                results.Add(new EmailSendResult(recipient, true));
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Failed to send bulk email [{Type}] to {Recipient}",
                    type,
                    item.RecipientEmail);

                results.Add(new EmailSendResult(
                    item.RecipientEmail,
                    false,
                    ex.Message));
            }
        }

        try
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to disconnect SMTP client after bulk email [{Type}]", type);
        }

        if (logs.Count > 0)
        {
            try
            {
                dbContext.EmailLogs.AddRange(logs);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to persist bulk email logs [{Type}]", type);
            }
        }

        return results;
    }

    private async Task ConnectAsync(SmtpClient client, CancellationToken cancellationToken)
    {
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
        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await client.AuthenticateAsync(
                _settings.UserName,
                _settings.Password,
                cancellationToken);
    }

    private static string NormalizeRecipient(string recipientEmail)
    {
        var recipient = recipientEmail.Trim();
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient email must not be empty.", nameof(recipientEmail));

        return recipient;
    }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

    private static void ValidateContent(EmailContent content)
    {
        if (string.IsNullOrWhiteSpace(content.Subject))
            throw new ArgumentException("Email subject must not be empty.", nameof(content));
    }
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

    private MimeMessage BuildMessage(string recipient, EmailContent content)
    {
        var message = new MimeMessage
    private async Task SaveLogSafelyAsync(EmailLog emailLog)
    {
        try
        {
            MessageId = MimeUtils.GenerateMessageId(),
            Subject = content.Subject,
            Body = BuildMessageBody(content)
        };

        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipient));

        return message;
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

    private EmailLog CreateLog(string recipient, EmailContent content, EmailType type) => new()
    {
        SenderEmail = _settings.SenderEmail,
        RecipientEmail = recipient,
        Subject = content.Subject,
        Body = content.TextBody,
        Type = type,
        SentAt = DateTime.UtcNow
    };

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
