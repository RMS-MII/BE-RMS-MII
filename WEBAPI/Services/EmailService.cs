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

public class EmailService(RmsDbContext dbContext, IConfiguration configuration) : IEmailService, IEmailRetryProcessor
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
            HtmlBody = content.HtmlBody,
            Type = type,
            Status = EmailDeliveryStatus.Sent,
            AttemptedAt = attemptedAt,
            SentAt = null
        };

        try
        {
            await SendMessageAsync(recipient, content, cancellationToken);

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

    public async Task<IReadOnlyList<EmailSendResult>> SendBulkEmailAsync(
        IReadOnlyCollection<BulkEmailMessage> messages,
        EmailType type,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return [];

        var results = new List<EmailSendResult>(messages.Count);
        var logs = new List<EmailLog>(messages.Count);
        using var client = new SmtpClient();

        try
        {
            await ConnectAsync(client, cancellationToken);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to connect to SMTP server for bulk email [{Type}]", type);
            foreach (var item in messages)
            {
                results.Add(new EmailSendResult(item.RecipientEmail, false, exception.Message));
                logs.Add(CreateBulkLog(item, type, EmailDeliveryStatus.Failed, exception));
            }
            await SaveLogsSafelyAsync(logs);
            return results;
        }

        foreach (var item in messages)
        {
            try
            {
                var recipient = NormalizeRecipient(item.RecipientEmail);
                ValidateContent(item.Content);
                await client.SendAsync(BuildMessage(recipient, item.Content), cancellationToken);
                results.Add(new EmailSendResult(recipient, true));
                logs.Add(CreateBulkLog(item, type, EmailDeliveryStatus.Sent));
                Log.Information("Sent email [{Type}] to {Recipient}: {Subject}", type, recipient, item.Content.Subject);
            }
            catch (Exception exception)
            {
                results.Add(new EmailSendResult(item.RecipientEmail, false, exception.Message));
                logs.Add(CreateBulkLog(item, type, EmailDeliveryStatus.Failed, exception));
                Log.Error(exception, "Failed to send bulk email [{Type}] to {Recipient}", type, item.RecipientEmail);
            }
        }

        try
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "Failed to disconnect SMTP client after bulk email [{Type}]", type);
        }

        await SaveLogsSafelyAsync(logs);
        return results;
    }

    public async Task<bool> ProcessNextPendingAsync(CancellationToken cancellationToken)
    {
        var nextId = await dbContext.EmailLogs
            .AsNoTracking()
            .Where(log => log.Status == EmailDeliveryStatus.Pending)
            .OrderBy(log => log.AttemptedAt)
            .Select(log => (Guid?)log.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextId is null)
            return false;

        var claimed = await dbContext.EmailLogs
            .Where(log => log.Id == nextId && log.Status == EmailDeliveryStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(log => log.Status, EmailDeliveryStatus.Sending)
                .SetProperty(log => log.AttemptedAt, DateTime.UtcNow), cancellationToken);

        if (claimed == 0)
            return true;

        var retryLog = await dbContext.EmailLogs.FirstAsync(log => log.Id == nextId, cancellationToken);
        try
        {
            var content = new EmailContent(retryLog.Subject, retryLog.Body, retryLog.HtmlBody);
            await SendMessageAsync(retryLog.RecipientEmail, content, cancellationToken);
            retryLog.Status = EmailDeliveryStatus.Sent;
            retryLog.SentAt = DateTime.UtcNow;
            retryLog.ErrorMessage = null;
        }
        catch (Exception exception)
        {
            retryLog.Status = EmailDeliveryStatus.Failed;
            retryLog.ErrorMessage = Truncate(exception.GetBaseException().Message, 1000);
            Log.Error(exception, "Failed to retry email log {EmailLogId} to {Recipient}", retryLog.Id, retryLog.RecipientEmail);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SendMessageAsync(string recipient, EmailContent content, CancellationToken cancellationToken)
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
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.Security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task ConnectAsync(SmtpClient client, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.Security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
    }

    private MimeMessage BuildMessage(string recipient, EmailContent content)
    {
        var message = new MimeMessage
        {
            MessageId = MimeUtils.GenerateMessageId(),
            Subject = content.Subject,
            Body = BuildMessageBody(content)
        };
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        return message;
    }

    private static string NormalizeRecipient(string recipientEmail)
    {
        var recipient = recipientEmail.Trim();
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient email must not be empty.", nameof(recipientEmail));
        return recipient;
    }

    private static void ValidateContent(EmailContent content)
    {
        if (string.IsNullOrWhiteSpace(content.Subject))
            throw new ArgumentException("Email subject must not be empty.", nameof(content));
    }

    private EmailLog CreateBulkLog(BulkEmailMessage item, EmailType type, EmailDeliveryStatus status, Exception? error = null)
    {
        var now = DateTime.UtcNow;
        return new EmailLog
        {
            SenderEmail = _settings.SenderEmail,
            RecipientEmail = item.RecipientEmail.Trim(),
            Subject = item.Content.Subject,
            Body = item.Content.TextBody,
            HtmlBody = item.Content.HtmlBody,
            Type = type,
            Status = status,
            AttemptedAt = now,
            SentAt = status == EmailDeliveryStatus.Sent ? now : null,
            ErrorMessage = error is null ? null : Truncate(error.GetBaseException().Message, 1000)
        };
    }

    private async Task SaveLogsSafelyAsync(IReadOnlyCollection<EmailLog> emailLogs)
    {
        try
        {
            dbContext.EmailLogs.AddRange(emailLogs);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            foreach (var emailLog in emailLogs)
                dbContext.Entry(emailLog).State = EntityState.Detached;
            Log.Error(exception, "Failed to persist bulk email delivery logs");
        }
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
