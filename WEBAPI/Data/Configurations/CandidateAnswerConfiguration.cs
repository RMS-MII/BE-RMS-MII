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

        // Snapshot tipe DISC diambil saat kandidat menjawab, sehingga perhitungan
        // ringkasan DISC tidak bergantung pada relasi ke tabel Question.
        builder.Property(x => x.DiscType)
            .HasConversion<int>()
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
