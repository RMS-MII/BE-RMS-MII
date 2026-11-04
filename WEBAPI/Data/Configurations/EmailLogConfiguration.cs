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

        builder.Property(x => x.Subject)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.SentAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => x.SentAt);
    }
}
