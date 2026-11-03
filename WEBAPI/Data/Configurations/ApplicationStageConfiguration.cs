using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class ApplicationStageConfiguration : IEntityTypeConfiguration<ApplicationStage>
{
    public void Configure(EntityTypeBuilder<ApplicationStage> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(a => a.Notes)
            .HasMaxLength(500);
        builder.Property(a => a.StartedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(a => a.CompletedAt)
            .HasColumnType("datetime2");
        builder.Property(a => a.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(a => a.UpdatedAt)
            .HasColumnType("datetime2");
        builder.Property(a => a.DurationTest)
            .HasColumnType("time");
        builder.Property(a => a.TotalScore)
            .HasColumnType("decimal(18,2)");
        builder.Property(a => a.EndAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(a => a.IsAttempts)
            .HasDefaultValue(false);

        builder.HasIndex(a => new { a.BatchApplicationId, a.BatchStageId })
            .IsUnique();

        builder.HasIndex(a => a.BatchApplicationId);
        
        //relationships
        builder.HasOne(a => a.BatchApplication)
            .WithMany(b => b.ApplicationStages)
            .HasForeignKey(a => a.BatchApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.BatchStage)
            .WithMany(bs => bs.ApplicationStages)
            .HasForeignKey(a => a.BatchStageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
