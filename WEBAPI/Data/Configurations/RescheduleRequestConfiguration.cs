using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Configurations;

public class RescheduleRequestConfiguration : IEntityTypeConfiguration<RescheduleRequest>
{
    public void Configure(EntityTypeBuilder<RescheduleRequest> builder)
    {
        builder.ToTable("RescheduleRequest");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InterviewSessionId)
            .IsRequired();

        builder.Property(x => x.RequesterEmail)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProposedStart)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ProposedEnd)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(100)
            .IsRequired();

        // Enum RescheduleStatus disimpan sebagai int di database
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ReviewedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.ReviewNotes)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.InterviewSessionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.InterviewSessionId, x.Status })
            .IsUnique()
            .HasFilter($"[Status] = {(int)RescheduleStatus.Pending}");
    }
}
