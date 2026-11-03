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
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(b => b.AppliedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(b => b.FinalResult)
            .HasMaxLength(500);
        builder.Property(b => b.Status)
            .IsRequired();
        builder.Property(b => b.FileOfferingLetter)
            .HasMaxLength(500);
        builder.Property(b => b.OfferingLetterStatus)
            .IsRequired();
        builder.Property(b => b.Notes)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(b => b.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(b => b.UpdatedAt)
            .HasColumnType("datetime2");
        
        //type check
        builder.HasIndex(b => b.CodeApplication)
            .IsUnique();
        
        //foreign key
        builder.HasOne(b => b.Candidate)
            .WithMany(c => c.BatchApplications)
            .HasForeignKey(b => b.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}