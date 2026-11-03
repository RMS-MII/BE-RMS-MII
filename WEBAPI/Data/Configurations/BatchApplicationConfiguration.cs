using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class BatchApplicationConfiguration : IEntityTypeConfiguration<BatchApplication>
{
    public void Configure(EntityTypeBuilder<BatchApplication> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.CodeApplication)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(b => b.AppliedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(b => b.FinalResultStatus)
            .HasConversion<int>();
        builder.Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(b => b.FileOfferingLetter)
            .HasMaxLength(500);
        builder.Property(b => b.OfferingLetterStatus)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(b => b.Notes)
            .HasMaxLength(500);
        builder.Property(b => b.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(b => b.UpdatedAt)
            .HasColumnType("datetime2");
        
        //type check
        builder.HasIndex(b => b.CodeApplication)
            .IsUnique();

        builder.HasIndex(b => new { b.CandidateId, b.RecruitmentBatchId })
            .IsUnique();

        builder.HasIndex(b => b.RecruitmentBatchId);
        
        //foreign key
        builder.HasOne(b => b.Candidate)
            .WithMany(c => c.BatchApplications)
            .HasForeignKey(b => b.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.RecruitmentBatch)
            .WithMany(rb => rb.BatchApplications)
            .HasForeignKey(b => b.RecruitmentBatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
