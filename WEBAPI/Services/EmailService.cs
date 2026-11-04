using Serilog;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

public class EmailService(RmsDbContext dbContext, IConfiguration configuration) : IEmailService
{
    private readonly string _senderEmail = configuration["MailSettings:SenderEmail"] ?? "no-reply@rms.local";

    public async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string body,
        EmailType type,
        CancellationToken cancellationToken = default)
    {
        Log.Information("Sending email [{Type}] to {Recipient}: {Subject}", type, recipientEmail, subject);

        // Catat ke EmailLog sesuai aturan bisnis (Requirement 12)
        var log = new EmailLog
        {
            SenderEmail = _senderEmail,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            Type = type,
            SentAt = DateTime.UtcNow
        };

        dbContext.EmailLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
