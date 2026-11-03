using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace WEBAPI.Data;

public class RmsDbContext(DbContextOptions<RmsDbContext> options) : DbContext(options)
{
    //isi entites object disni
    // public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //saya mau configurasinya bukan di dbContext
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}