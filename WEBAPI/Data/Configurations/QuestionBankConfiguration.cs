using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
{
    public void Configure(EntityTypeBuilder<QuestionBank> builder)
    {
        builder.ToTable("QuestionBank");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DepartmentId)
            .IsRequired();

        builder.Property(x => x.StageTypeId)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.Questions)
            .WithOne(x => x.QuestionBank)
            .HasForeignKey(x => x.QuestionBankId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
