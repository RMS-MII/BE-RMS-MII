using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEBAPI.Entities;

namespace WEBAPI.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Code)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(d => d.Description)
            .HasMaxLength(500);
        
        //check
        builder.HasIndex(d => d.Code)
            .IsUnique();
    }
}