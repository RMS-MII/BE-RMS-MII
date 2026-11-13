using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetToken");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.TokenHash)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.Property(p => p.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
        
        builder.Property(p => p.ExpiresAt)
            .HasColumnType("datetime2")
            .IsRequired();
        
        builder.Property(p => p.UsedAt)
            .HasColumnType("datetime2");
        
        builder.HasIndex(p => p.TokenHash).IsUnique();
        builder.HasIndex(p => p.AccountId);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}