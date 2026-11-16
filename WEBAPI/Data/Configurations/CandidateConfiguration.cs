using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.IsInTalentPool).HasDefaultValue(true).HasSentinel(true);

        builder.Property(c => c.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .IsRequired();

        // Enum Gender disimpan sebagai int di database
        builder.Property(c => c.Gender)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Address)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Education)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.BirthDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.ExpectedSalary)
            .HasColumnType("money")
            .IsRequired();

        builder.Property(c => c.Cv)
            .HasMaxLength(200);

        builder.Property(c => c.Portfolio)
            .HasMaxLength(200);

        builder.Property(c => c.Gpa)
            .HasColumnType("decimal(3,2)")
            .IsRequired();

        builder.Property(c => c.Institution)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("datetime2");
        
        // Indexes
        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();
    }
}
