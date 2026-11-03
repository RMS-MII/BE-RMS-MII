using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class QuestionOptionsConfiguration : IEntityTypeConfiguration<QuestionOptions>
{
    public void Configure(EntityTypeBuilder<QuestionOptions> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionId)
            .IsRequired();

        builder.Property(x => x.OptionLabel)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.OptionText)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Question)
            .WithMany(x => x.QuestionOptions)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
