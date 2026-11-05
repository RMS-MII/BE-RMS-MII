using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class CandidateAnswerConfiguration : IEntityTypeConfiguration<CandidateAnswer>
{
    public void Configure(EntityTypeBuilder<CandidateAnswer> builder)
    {
        builder.ToTable("CandidateAnswer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationStageId)
            .IsRequired();

        builder.Property(x => x.QuestionText)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.QuestionOptionText)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.ApplicationStageId);

        builder.HasOne(x => x.ApplicationStage)
            .WithMany(x => x.CandidateAnswers)
            .HasForeignKey(x => x.ApplicationStageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
