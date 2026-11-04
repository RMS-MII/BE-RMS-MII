using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class InterviewAssessmentConfiguration : IEntityTypeConfiguration<InterviewAssessment>
{
    public void Configure(EntityTypeBuilder<InterviewAssessment> builder)
    {
        builder.ToTable("InterviewAssessment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionInterviewId)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnType("nvarchar(max)");

        // Enum Recommendation disimpan sebagai int di database
        builder.Property(x => x.Recommendation)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        // Satu penilaian per pewawancara per sesi
        builder.HasIndex(x => x.SessionInterviewId)
            .IsUnique();

        builder.HasOne(x => x.SessionInterview)
            .WithOne(x => x.InterviewAssessment)
            .HasForeignKey<InterviewAssessment>(x => x.SessionInterviewId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
