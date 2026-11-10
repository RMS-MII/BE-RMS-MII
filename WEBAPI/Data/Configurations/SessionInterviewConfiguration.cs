using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class SessionInterviewConfiguration : IEntityTypeConfiguration<SessionInterview>
{
    public void Configure(EntityTypeBuilder<SessionInterview> builder)
    {
        builder.ToTable("SessionInterview");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StageEmployeeId)
            .IsRequired();

        builder.Property(x => x.InterviewSessionId)
            .IsRequired();

        // Enum SessionInterviewStatus disimpan sebagai int di database
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsLead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        // Satu pewawancara hanya terdaftar sekali pada sesi yang sama
        builder.HasIndex(x => new { x.StageEmployeeId, x.InterviewSessionId })
            .IsUnique();

        builder.HasIndex(x => x.StageEmployeeId);
        builder.HasIndex(x => x.InterviewSessionId);

        // Satu sesi wawancara hanya boleh memiliki satu lead interviewer
        builder.HasIndex(x => x.InterviewSessionId)
            .IsUnique()
            .HasFilter("[IsLead] = 1")
            .HasDatabaseName("UX_SessionInterview_OneLeadPerSession");

        builder.HasOne(x => x.StageEmployee)
            .WithMany(x => x.SessionInterviews)
            .HasForeignKey(x => x.StageEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InterviewSession)
            .WithMany(x => x.SessionInterviews)
            .HasForeignKey(x => x.InterviewSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satu SessionInterview hanya memiliki satu InterviewAssessment
        builder.HasOne(x => x.InterviewAssessment)
            .WithOne(x => x.SessionInterview)
            .HasForeignKey<InterviewAssessment>(x => x.SessionInterviewId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
