using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public sealed class PublicApplicationVerificationConfiguration : IEntityTypeConfiguration<PublicApplicationVerification>
{
    public void Configure(EntityTypeBuilder<PublicApplicationVerification> builder)
    {
        builder.ToTable("PublicApplicationVerification");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Email).HasMaxLength(100).IsRequired();
        builder.Property(item => item.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(item => new { item.Email, item.CreatedAt });
        builder.HasOne<RecruitmentBatch>().WithMany().HasForeignKey(item => item.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
