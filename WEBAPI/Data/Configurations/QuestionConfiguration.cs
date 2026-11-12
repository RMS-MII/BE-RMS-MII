using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Question");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionBankId)
            .IsRequired();

        builder.Property(x => x.QuestionText)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // Enum QuestionType disimpan sebagai int di database
        builder.Property(x => x.QuestionType)
            .HasConversion<int>()
            .IsRequired();

        // Enum DiscType disimpan sebagai int di database
        builder.Property(x => x.DiscType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(true);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.QuestionBank)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.QuestionBankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.QuestionOptions)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
