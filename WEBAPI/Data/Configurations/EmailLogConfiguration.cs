using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderEmail)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(50)
            .IsRequired();

        // Enum EmailType disimpan sebagai int di database
        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue(WEBAPI.Entities.Enums.EmailDeliveryStatus.Sent)
            .HasSentinel((WEBAPI.Entities.Enums.EmailDeliveryStatus)0)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.HtmlBody)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AttemptedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.SentAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.AttemptedAt);
        builder.HasIndex(x => new { x.Status, x.AttemptedAt });
    }
}
