using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.ToTable("InterviewSession");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ScheduledStart)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ScheduledEnd)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.IsOnline)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(100);

        builder.Property(x => x.MeetingUrl)
            .HasMaxLength(100);

        // Enum InterviewSessionStatus disimpan sebagai int di database
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.FileSubmission)
            .HasMaxLength(100);

        builder.Property(x => x.EndedSubmissionAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.ApplicationStageId);

        builder.HasIndex(x => x.ScheduledStart);

        // Satu ApplicationStage boleh memiliki banyak InterviewSession (reschedule)
        builder.HasOne(x => x.ApplicationStage)
            .WithMany(x => x.InterviewSessions)
            .HasForeignKey(x => x.ApplicationStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SessionInterviews)
            .WithOne(x => x.InterviewSession)
            .HasForeignKey(x => x.InterviewSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
