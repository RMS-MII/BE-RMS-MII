using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Entities;

namespace WEBAPI.Data;

public class RmsDbContext(DbContextOptions<RmsDbContext> options) : DbContext(options)
{
    // Question Domain Entities
    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOptions> QuestionOptions => Set<QuestionOptions>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Menjalankan semua IEntityTypeConfiguration yang ada di assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
