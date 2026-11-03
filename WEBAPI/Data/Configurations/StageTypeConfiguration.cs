using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class StageTypeConfiguration : IEntityTypeConfiguration<StageType>
{
    public void Configure(EntityTypeBuilder<StageType> builder)
    {
        builder.ToTable("StageType");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(100);

        // Enum StageCategory disimpan sebagai int di database
        builder.Property(x => x.StageCategory)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.BatchStages)
            .WithOne(x => x.StageType)
            .HasForeignKey(x => x.StageTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
