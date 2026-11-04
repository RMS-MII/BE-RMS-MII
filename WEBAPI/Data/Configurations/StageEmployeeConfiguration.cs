using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class StageEmployeeConfiguration : IEntityTypeConfiguration<StageEmployee>
{
    public void Configure(EntityTypeBuilder<StageEmployee> builder)
    {
        builder.ToTable("StageEmployee");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchStageId)
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        // Enum StageEmployeeStatus disimpan sebagai int di database
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.Note)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        // Satu employee hanya ditugaskan sekali pada satu batch stage
        builder.HasIndex(x => new { x.BatchStageId, x.EmployeeId })
            .IsUnique();

        builder.HasIndex(x => x.BatchStageId);
        builder.HasIndex(x => x.EmployeeId);

        builder.HasOne(x => x.BatchStage)
            .WithMany(x => x.StageEmployees)
            .HasForeignKey(x => x.BatchStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.StageEmployees)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SessionInterviews)
            .WithOne(x => x.StageEmployee)
            .HasForeignKey(x => x.StageEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
