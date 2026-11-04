using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Entities;

namespace WEBAPI.Data;

public class RmsDbContext(DbContextOptions<RmsDbContext> options) : DbContext(options)
{
    
    //Candidate domain Entities
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<BatchApplication> BatchApplications => Set<BatchApplication>();
    public DbSet<ApplicationStage> ApplicationStages => Set<ApplicationStage>();
    
    // Organization Domain Entities
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Account> Accounts => Set<Account>();

    // Question Domain Entities
    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOptions> QuestionOptions => Set<QuestionOptions>();

    // Interview Domain Entities
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<StageEmployee> StageEmployees => Set<StageEmployee>();
    public DbSet<SessionInterview> SessionInterviews => Set<SessionInterview>();
    public DbSet<InterviewAssessment> InterviewAssessments => Set<InterviewAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Menjalankan semua IEntityTypeConfiguration yang ada di assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
