using Serilog;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

public class EmailService(RmsDbContext dbContext, IConfiguration configuration) : IEmailService
{
    private readonly string _senderEmail = configuration["MailSettings:SenderEmail"] ?? "no-reply@rms.local";

    public Task SendEmailAsync(
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

        // Hanya Add ke change tracker; SaveChangesAsync dilakukan oleh handler pemanggil
        // agar semua perubahan (bisnis + log email) tersimpan dalam satu transaksi
        dbContext.EmailLogs.Add(log);

        return Task.CompletedTask;
    }
}
