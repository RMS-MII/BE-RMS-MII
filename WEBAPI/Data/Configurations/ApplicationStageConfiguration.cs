using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class ApplicationStageConfiguration : IEntityTypeConfiguration<ApplicationStage>
{
    public void Configure(EntityTypeBuilder<ApplicationStage> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Notes)
            .HasMaxLength(500);
        builder.Property(a => a.CompletedAt)
            .HasColumnType("datetime2");
        builder.Property(a => a.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(a => a.UpdatedAt)
            .HasColumnType("datetime2");
        builder.Property(a => a.DurationTest)
            .HasColumnType("int");
        builder.Property(a => a.TotalScore)
            .HasColumnType("decimal");
        builder.Property(a => a.EndAt)
            .HasColumnType("datetime2");
        builder.Property(a => a.IsAttempt)
            .HasColumnType("bool");
        
        //relathionship

        builder.HasOne(a => a.BatchApplication)
            .WithMany(b => b.ApplicationStages)
            .HasForeignKey(a => a.BatchApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}