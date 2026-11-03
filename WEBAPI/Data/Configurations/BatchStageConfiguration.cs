using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class BatchStageConfiguration : IEntityTypeConfiguration<BatchStage>
{
    public void Configure(EntityTypeBuilder<BatchStage> builder)
    {
        builder.ToTable("BatchStage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StageName)
            .HasMaxLength(50)
            .IsRequired();

        // Constraint Unique (RecruitmentBatchId, StageName)
        builder.HasIndex(x => new { x.RecruitmentBatchId, x.StageName })
            .IsUnique();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.PassingScore)
            .HasPrecision(18, 2);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Self-referencing FK ke NextStage
        builder.HasOne(x => x.NextStage)
            .WithMany(x => x.PreviousStages)
            .HasForeignKey(x => x.NextStageId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK ke RecruitmentBatch
        builder.HasOne(x => x.RecruitmentBatch)
            .WithMany(x => x.BatchStages)
            .HasForeignKey(x => x.RecruitmentBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK ke StageType
        builder.HasOne(x => x.StageType)
            .WithMany(x => x.BatchStages)
            .HasForeignKey(x => x.StageTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relasi ke ApplicationStages
        builder.HasMany(x => x.ApplicationStages)
            .WithOne(x => x.BatchStage)
            .HasForeignKey(x => x.BatchStageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
