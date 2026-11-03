using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class RecruitmentBatchConfiguration : IEntityTypeConfiguration<RecruitmentBatch>
{
    public void Configure(EntityTypeBuilder<RecruitmentBatch> builder)
    {
        builder.ToTable("RecruitmentBatch");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DepartmentId)
            .IsRequired();

        builder.Property(x => x.BatchCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.BatchCode)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        // Enum BatchStatus disimpan sebagai int di database
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // FK ke Department
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relasi ke BatchStages
        builder.HasMany(x => x.BatchStages)
            .WithOne(x => x.RecruitmentBatch)
            .HasForeignKey(x => x.RecruitmentBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relasi ke BatchApplications
        builder.HasMany(x => x.BatchApplications)
            .WithOne(x => x.RecruitmentBatch)
            .HasForeignKey(x => x.RecruitmentBatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
