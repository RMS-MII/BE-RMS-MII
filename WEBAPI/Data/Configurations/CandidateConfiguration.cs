using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.Gender)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(c => c.Address)
            .HasMaxLength(200)
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
            .IsRequired();
        builder.Property(c => c.Institution)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(c => c.UpdatedAt)
            .HasColumnType("datetime2");
        
        //type check
        builder.HasIndex(c => c.Email)
            .IsUnique();
        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();
    }
}