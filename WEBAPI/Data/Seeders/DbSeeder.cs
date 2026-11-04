using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Services;

namespace WEBAPI.Data.Seeders;

/// <summary>
/// Idempotent development seeder. Fixed GUIDs keep the data stable across machines.
/// </summary>
public sealed class DbSeeder(
    RmsDbContext db,
    IPasswordHasher<Account> passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration,
    ILogger<DbSeeder> logger)
{
    private static readonly Guid TechnologyDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EngineeringDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly Guid FinanceDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111113");
    private static readonly Guid HrDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111114");

    private static readonly Guid HrRoleId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    private static readonly Guid InterviewerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid HrEmployeeOneId = Guid.Parse("33333333-3333-3333-3333-333333333331");
    private static readonly Guid HrEmployeeTwoId = Guid.Parse("33333333-3333-3333-3333-333333333332");
    private static readonly Guid InterviewerOneEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid InterviewerTwoEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333334");
    private static readonly Guid InterviewerThreeEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333335");
    private static readonly Guid InterviewerFourEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333336");

    private static readonly Guid HrAccountOneId = Guid.Parse("44444444-4444-4444-4444-444444444441");
    private static readonly Guid HrAccountTwoId = Guid.Parse("44444444-4444-4444-4444-444444444442");
    private static readonly Guid InterviewerOneAccountId = Guid.Parse("44444444-4444-4444-4444-444444444443");
    private static readonly Guid InterviewerTwoAccountId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid InterviewerThreeAccountId = Guid.Parse("44444444-4444-4444-4444-444444444445");
    private static readonly Guid InterviewerFourAccountId = Guid.Parse("44444444-4444-4444-4444-444444444446");

    private static readonly Guid PsikotesStageTypeId = Guid.Parse("55555555-5555-5555-5555-555555555551");
    private static readonly Guid TechnicalTestStageTypeId = Guid.Parse("55555555-5555-5555-5555-555555555552");
    private static readonly Guid HrInterviewStageTypeId = Guid.Parse("55555555-5555-5555-5555-555555555553");
    private static readonly Guid UserInterviewStageTypeId = Guid.Parse("55555555-5555-5555-5555-555555555554");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        await SeedRolesAsync(now, ct);
        await SeedDepartmentsAsync(now, ct);
        await SeedEmployeesAsync(now, ct);
        await SeedAccountsAsync(now, ct);
        await SeedStageTypesAsync(now, ct);

        logger.LogInformation("Database seeding selesai");
    }

    private async Task SeedRolesAsync(DateTime now, CancellationToken ct)
    {
        var changed = false;

        if (!await db.Roles.AnyAsync(x => x.Id == HrRoleId, ct))
        {
            db.Roles.Add(new Role
            {
                Id = HrRoleId,
                Name = RmsRoles.Hr,
                Description = "Human Resources",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Roles.AnyAsync(x => x.Id == InterviewerRoleId, ct))
        {
            db.Roles.Add(new Role
            {
                Id = InterviewerRoleId,
                Name = RmsRoles.Interviewer,
                Description = "Interviewer",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);
    }

    private async Task SeedDepartmentsAsync(DateTime now, CancellationToken ct)
    {
        var changed = false;

        if (!await db.Departments.AnyAsync(x => x.Id == TechnologyDepartmentId, ct))
        {
            db.Departments.Add(new Department
            {
                Id = TechnologyDepartmentId,
                Code = "TECH",
                Name = "Technology",
                Description = "Technology division",
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Departments.AnyAsync(x => x.Id == EngineeringDepartmentId, ct))
        {
            db.Departments.Add(new Department
            {
                Id = EngineeringDepartmentId,
                ParentDepartmentId = TechnologyDepartmentId,
                Code = "ENG",
                Name = "Engineering",
                Description = "Engineering sub-department",
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Departments.AnyAsync(x => x.Id == FinanceDepartmentId, ct))
        {
            db.Departments.Add(new Department
            {
                Id = FinanceDepartmentId,
                Code = "FIN",
                Name = "Finance",
                Description = "Finance division",
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Departments.AnyAsync(x => x.Id == HrDepartmentId, ct))
        {
            db.Departments.Add(new Department
            {
                Id = HrDepartmentId,
                Code = "HR",
                Name = "Human Resources",
                Description = "Human Resources division",
                CreatedAt = now
            });
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);
    }

    private async Task SeedEmployeesAsync(DateTime now, CancellationToken ct)
    {
        var changed = false;

        if (!await db.Employees.AnyAsync(x => x.Id == HrEmployeeOneId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = HrEmployeeOneId,
                DepartmentId = HrDepartmentId,
                EmployeeNumber = "EMP-HR-001",
                FullName = "HR Manager",
                Email = "hr1@rms.local",
                Gender = Gender.Female,
                JobTitle = "HR Manager",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Employees.AnyAsync(x => x.Id == HrEmployeeTwoId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = HrEmployeeTwoId,
                ManagerId = HrEmployeeOneId,
                DepartmentId = HrDepartmentId,
                EmployeeNumber = "EMP-HR-002",
                FullName = "HR Specialist",
                Email = "hr2@rms.local",
                Gender = Gender.Male,
                JobTitle = "HR Specialist",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Employees.AnyAsync(x => x.Id == InterviewerOneEmployeeId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = InterviewerOneEmployeeId,
                ManagerId = HrEmployeeOneId,
                DepartmentId = TechnologyDepartmentId,
                EmployeeNumber = "EMP-INT-001",
                FullName = "Interviewer Technology",
                Email = "interviewer1@rms.local",
                Gender = Gender.Male,
                JobTitle = "Engineering Manager",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Employees.AnyAsync(x => x.Id == InterviewerTwoEmployeeId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = InterviewerTwoEmployeeId,
                ManagerId = HrEmployeeOneId,
                DepartmentId = EngineeringDepartmentId,
                EmployeeNumber = "EMP-INT-002",
                FullName = "Interviewer Engineering",
                Email = "interviewer2@rms.local",
                Gender = Gender.Female,
                JobTitle = "Senior Engineer",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Employees.AnyAsync(x => x.Id == InterviewerThreeEmployeeId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = InterviewerThreeEmployeeId,
                ManagerId = HrEmployeeOneId,
                DepartmentId = FinanceDepartmentId,
                EmployeeNumber = "EMP-INT-003",
                FullName = "Interviewer Finance",
                Email = "interviewer3@rms.local",
                Gender = Gender.Female,
                JobTitle = "Finance Manager",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.Employees.AnyAsync(x => x.Id == InterviewerFourEmployeeId, ct))
        {
            db.Employees.Add(new Employee
            {
                Id = InterviewerFourEmployeeId,
                ManagerId = HrEmployeeOneId,
                DepartmentId = HrDepartmentId,
                EmployeeNumber = "EMP-INT-004",
                FullName = "Interviewer Human Resources",
                Email = "interviewer4@rms.local",
                Gender = Gender.Male,
                JobTitle = "HR Business Partner",
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);
    }

    private async Task SeedAccountsAsync(DateTime now, CancellationToken ct)
    {
        var defaultPassword = configuration["Seed:DefaultPassword"];
        if (string.IsNullOrWhiteSpace(defaultPassword))
        {
            logger.LogWarning("Seed:DefaultPassword tidak diatur; seeding akun dilewati");
            return;
        }

        var changed = false;

        changed |= await AddAccountAsync(
            HrAccountOneId, HrEmployeeOneId, HrRoleId, "hr1", defaultPassword, now, ct);
        changed |= await AddAccountAsync(
            HrAccountTwoId, HrEmployeeTwoId, HrRoleId, "hr2", defaultPassword, now, ct);
        changed |= await AddAccountAsync(
            InterviewerOneAccountId, InterviewerOneEmployeeId, InterviewerRoleId,
            "interviewer1", defaultPassword, now, ct);
        changed |= await AddAccountAsync(
            InterviewerTwoAccountId, InterviewerTwoEmployeeId, InterviewerRoleId,
            "interviewer2", defaultPassword, now, ct);
        changed |= await AddAccountAsync(
            InterviewerThreeAccountId, InterviewerThreeEmployeeId, InterviewerRoleId,
            "interviewer3", defaultPassword, now, ct);
        changed |= await AddAccountAsync(
            InterviewerFourAccountId, InterviewerFourEmployeeId, InterviewerRoleId,
            "interviewer4", defaultPassword, now, ct);

        if (changed)
            await db.SaveChangesAsync(ct);
    }

    private async Task<bool> AddAccountAsync(
        Guid id, Guid employeeId, Guid roleId, string username, string password,
        DateTime now, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(x => x.Id == id, ct))
            return false;

        var account = new Account
        {
            Id = id,
            EmployeeId = employeeId,
            RoleId = roleId,
            Username = username,
            IsActive = true,
            CreatedAt = now
        };
        account.PasswordHash = passwordHasher.HashPassword(account, password);
        db.Accounts.Add(account);
        return true;
    }

    private async Task SeedStageTypesAsync(DateTime now, CancellationToken ct)
    {
        var changed = false;

        if (!await db.StageTypes.AnyAsync(x => x.Id == PsikotesStageTypeId, ct))
        {
            db.StageTypes.Add(new StageType
            {
                Id = PsikotesStageTypeId,
                Name = "Psikotes",
                Description = "Psikotes",
                StageCategory = StageCategory.Test,
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.StageTypes.AnyAsync(x => x.Id == TechnicalTestStageTypeId, ct))
        {
            db.StageTypes.Add(new StageType
            {
                Id = TechnicalTestStageTypeId,
                Name = "Technical & Logical Test",
                Description = "Technical and logical test",
                StageCategory = StageCategory.Test,
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.StageTypes.AnyAsync(x => x.Id == HrInterviewStageTypeId, ct))
        {
            db.StageTypes.Add(new StageType
            {
                Id = HrInterviewStageTypeId,
                Name = "Interview HR",
                Description = "Interview with HR",
                StageCategory = StageCategory.Interview,
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (!await db.StageTypes.AnyAsync(x => x.Id == UserInterviewStageTypeId, ct))
        {
            db.StageTypes.Add(new StageType
            {
                Id = UserInterviewStageTypeId,
                Name = "Interview User",
                Description = "Interview with user department",
                StageCategory = StageCategory.Interview,
                IsActive = true,
                CreatedAt = now
            });
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(ct);
    }
}
