using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Seeders;

public static class DataSeeder
{
    #region Fixed Seed GUIDs

    // Departments
    public static readonly Guid DeptTechId = Guid.Parse("01930001-0000-7000-8000-000000000001");
    public static readonly Guid DeptSoftwareEngId = Guid.Parse("01930001-0000-7000-8000-000000000002");
    public static readonly Guid DeptQaId = Guid.Parse("01930001-0000-7000-8000-000000000003");
    public static readonly Guid DeptHrId = Guid.Parse("01930001-0000-7000-8000-000000000004");
    public static readonly Guid DeptFinanceId = Guid.Parse("01930001-0000-7000-8000-000000000005");

    // Roles
    public static readonly Guid RoleHrId = Guid.Parse("01930002-0000-7000-8000-000000000001");
    public static readonly Guid RoleInterviewerId = Guid.Parse("01930002-0000-7000-8000-000000000002");

    // Employees
    public static readonly Guid EmpHrManagerId = Guid.Parse("01930003-0000-7000-8000-000000000001");
    public static readonly Guid EmpVpEngId = Guid.Parse("01930003-0000-7000-8000-000000000002");
    public static readonly Guid EmpLeadDevId = Guid.Parse("01930003-0000-7000-8000-000000000003");
    public static readonly Guid EmpQaLeadId = Guid.Parse("01930003-0000-7000-8000-000000000004");
    public static readonly Guid EmpFinanceLeadId = Guid.Parse("01930003-0000-7000-8000-000000000005");

    // Accounts
    public static readonly Guid AccHrManagerId = Guid.Parse("01930004-0000-7000-8000-000000000001");
    public static readonly Guid AccVpEngId = Guid.Parse("01930004-0000-7000-8000-000000000002");
    public static readonly Guid AccLeadDevId = Guid.Parse("01930004-0000-7000-8000-000000000003");
    public static readonly Guid AccQaLeadId = Guid.Parse("01930004-0000-7000-8000-000000000004");
    public static readonly Guid AccFinanceLeadId = Guid.Parse("01930004-0000-7000-8000-000000000005");

    // Stage Types
    public static readonly Guid StageTypePsikotesId = Guid.Parse("01930005-0000-7000-8000-000000000001");
    public static readonly Guid StageTypeTechnicalTestId = Guid.Parse("01930005-0000-7000-8000-000000000002");
    public static readonly Guid StageTypeLogicalTestId = Guid.Parse("01930005-0000-7000-8000-000000000003");
    public static readonly Guid StageTypeInterviewHrId = Guid.Parse("01930005-0000-7000-8000-000000000004");
    public static readonly Guid StageTypeInterviewUserId = Guid.Parse("01930005-0000-7000-8000-000000000005");

    // Question Banks
    public static readonly Guid QBankPsikotesId = Guid.Parse("01930006-0000-7000-8000-000000000001");
    public static readonly Guid QBankLogicalId = Guid.Parse("01930006-0000-7000-8000-000000000002");
    public static readonly Guid QBankTechnicalId = Guid.Parse("01930006-0000-7000-8000-000000000003");

    // Sample Batch & Stage (untuk flow Wawancara)
    public static readonly Guid SampleBatchId = Guid.Parse("66666666-6666-6666-6666-666666666661");
    public static readonly Guid SampleInterviewStageId = Guid.Parse("77777777-7777-7777-7777-777777777771");
    public static readonly Guid SampleCandidateId = Guid.Parse("01930009-0000-7000-8000-000000000001");
    public static readonly Guid SampleBatchApplicationId = Guid.Parse("0193000a-0000-7000-8000-000000000001");
    public static readonly Guid SampleApplicationStageId = Guid.Parse("0193000b-0000-7000-8000-000000000001");
    public static readonly Guid SampleStageEmployeeId = Guid.Parse("0193000c-0000-7000-8000-000000000001");
    public static readonly Guid SampleInterviewSessionId = Guid.Parse("0193000d-0000-7000-8000-000000000001");
    public static readonly Guid SampleSessionInterviewId = Guid.Parse("0193000e-0000-7000-8000-000000000001");

    #endregion

    public static async Task SeedAsync(
        RmsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        bool includeSampleBatch = false)
    {
        Log.Information("Checking and running idempotent database seeder (IncludeSampleBatch={IncludeSampleBatch})...", includeSampleBatch);

        await SeedDepartmentsAsync(context);
        await SeedRolesAsync(context);
        await SeedEmployeesAsync(context);
        await SeedAccountsAsync(context, passwordHasher);
        await SeedStageTypesAsync(context);
        await SeedQuestionBanksAndQuestionsAsync(context);
        await PsychologyTestSeeder.SeedAsync(context);

        if (includeSampleBatch)
        {
            await SeedSampleBatchAndStageAsync(context);
        }

        Log.Information("Database seeder completed successfully.");
    }

    /// <summary>
    /// Checkpoint reset: membersihkan semua data pengujian development (transaksional dan testing)
    /// dari entitas anak ke entitas induk, menyisakan master data bersih untuk pengujian murni dari Frontend.
    /// </summary>
    public static async Task ResetToCheckpointAsync(
        RmsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        bool includeSampleBatch = false,
        bool clearPerformanceData = true)
    {
        Log.Information("Resetting database to seed checkpoint...");

        var previousTimeout = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(Math.Max(previousTimeout ?? 0, 90));

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            // 1. Bersihkan seluruh tabel transaksional dari entitas anak (child) ke entitas induk (parent)
            await context.CandidateAnswers.ExecuteDeleteAsync();
            await context.EmailLogs.ExecuteDeleteAsync();
            await context.InterviewAssessments.ExecuteDeleteAsync();
            await context.RescheduleRequests.ExecuteDeleteAsync();
            await context.SessionInterviews.ExecuteDeleteAsync();
            await context.InterviewSessions.ExecuteDeleteAsync();
            await context.StageEmployees.ExecuteDeleteAsync();
            await context.ApplicationStages.ExecuteDeleteAsync();
            await context.BatchApplications.ExecuteDeleteAsync();

            // Putus relasi self-referencing NextStageId sebelum menghapus BatchStages
            await context.BatchStages.ExecuteUpdateAsync(s => s.SetProperty(x => x.NextStageId, (Guid?)null));
            await context.BatchStages.ExecuteDeleteAsync();
            await context.PublicApplicationVerifications.Where(code => code.BatchId != null).ExecuteDeleteAsync();
            await context.RecruitmentBatches.ExecuteDeleteAsync();

            // 2. Bersihkan sisa data perf (kandidat/email bertanda perf) agar tidak tertinggal di checkpoint.
            if (clearPerformanceData)
                await PerformanceDataSeeder.ClearAsync(context);

            // 3. Pastikan master data terisi (dan hanya buat sample batch jika diaktifkan)
            await SeedAsync(context, passwordHasher, includeSampleBatch);
            await transaction.CommitAsync();

            Log.Information("Database checkpoint restored successfully. IncludeSampleBatch={IncludeSampleBatch}", includeSampleBatch);
        }
        finally
        {
            context.Database.SetCommandTimeout(previousTimeout);
        }
    }
    private static async Task SeedDepartmentsAsync(RmsDbContext context)
    {
        if (await context.Departments.AnyAsync()) return;

        var tech = new Department
        {
            Id = DeptTechId,
            Code = "TECH",
            Name = "Technology",
            Description = "Divisi Teknologi dan Rekayasa Sistem Perusahaan",
            ParentDepartmentId = null,
            CreatedAt = DateTime.UtcNow
        };

        var se = new Department
        {
            Id = DeptSoftwareEngId,
            Code = "SE",
            Name = "Software Engineering",
            Description = "Departemen Pengembangan Perangkat Lunak dan Aplikasi",
            ParentDepartmentId = DeptTechId,
            CreatedAt = DateTime.UtcNow
        };

        var qa = new Department
        {
            Id = DeptQaId,
            Code = "QA",
            Name = "Quality Assurance",
            Description = "Departemen Penjaminan Kualitas dan Pengujian Sistem",
            ParentDepartmentId = DeptTechId,
            CreatedAt = DateTime.UtcNow
        };

        var hr = new Department
        {
            Id = DeptHrId,
            Code = "HR",
            Name = "Human Resources",
            Description = "Departemen Manajemen Sumber Daya Manusia dan Rekrutmen",
            ParentDepartmentId = null,
            CreatedAt = DateTime.UtcNow
        };

        var fin = new Department
        {
            Id = DeptFinanceId,
            Code = "FIN",
            Name = "Finance",
            Description = "Departemen Keuangan dan Akuntansi",
            ParentDepartmentId = null,
            CreatedAt = DateTime.UtcNow
        };

        context.Departments.AddRange(tech, se, qa, hr, fin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RmsDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var hrRole = new Role
        {
            Id = RoleHrId,
            Name = "HR",
            Description = "Hak akses administrator HR dan pengelola rekrutmen",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var interviewerRole = new Role
        {
            Id = RoleInterviewerId,
            Name = "Interviewer",
            Description = "Hak akses pewawancara teknis dan user",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(hrRole, interviewerRole);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(RmsDbContext context)
    {
        if (await context.Employees.AnyAsync()) return;

        // 1. HR Manager
        var hrManager = new Employee
        {
            Id = EmpHrManagerId,
            DepartmentId = DeptHrId,
            ManagerId = null,
            EmployeeNumber = "EMP-HR-001",
            FullName = "Jane Doe",
            Email = "jane.doe@rms.local",
            Gender = Gender.Female,
            JobTitle = "HR Recruitment Manager",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 2. VP of Engineering (Manager untuk dev & QA)
        var vpEng = new Employee
        {
            Id = EmpVpEngId,
            DepartmentId = DeptTechId,
            ManagerId = null,
            EmployeeNumber = "EMP-TECH-001",
            FullName = "Budi Santoso",
            Email = "budi.santoso@rms.local",
            Gender = Gender.Male,
            JobTitle = "VP of Engineering",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Lead Software Engineer (Subordinate Budi Santoso)
        var leadDev = new Employee
        {
            Id = EmpLeadDevId,
            DepartmentId = DeptSoftwareEngId,
            ManagerId = EmpVpEngId,
            EmployeeNumber = "EMP-SE-001",
            FullName = "Fariz Irza",
            Email = "fariz.irza@rms.local",
            Gender = Gender.Male,
            JobTitle = "Lead Software Engineer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 4. QA Lead (Subordinate Budi Santoso)
        var qaLead = new Employee
        {
            Id = EmpQaLeadId,
            DepartmentId = DeptQaId,
            ManagerId = EmpVpEngId,
            EmployeeNumber = "EMP-QA-001",
            FullName = "Siti Rahma",
            Email = "siti.rahma@rms.local",
            Gender = Gender.Female,
            JobTitle = "QA Lead",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Finance Lead
        var finLead = new Employee
        {
            Id = EmpFinanceLeadId,
            DepartmentId = DeptFinanceId,
            ManagerId = null,
            EmployeeNumber = "EMP-FIN-001",
            FullName = "Ahmad Fauzi",
            Email = "ahmad.fauzi@rms.local",
            Gender = Gender.Male,
            JobTitle = "Finance Lead",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Employees.AddRange(hrManager, vpEng, leadDev, qaLead, finLead);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAccountsAsync(RmsDbContext context, IPasswordHasher<Account> passwordHasher)
    {
        if (await context.Accounts.AnyAsync()) return;

        var defaultPassword = "Password123!";

        var accounts = new List<Account>
        {
            new()
            {
                Id = AccHrManagerId,
                EmployeeId = EmpHrManagerId,
                RoleId = RoleHrId,
                Username = "hr_manager",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = AccVpEngId,
                EmployeeId = EmpVpEngId,
                RoleId = RoleInterviewerId,
                Username = "budi_santoso",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = AccLeadDevId,
                EmployeeId = EmpLeadDevId,
                RoleId = RoleInterviewerId,
                Username = "fariz_irza",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = AccQaLeadId,
                EmployeeId = EmpQaLeadId,
                RoleId = RoleInterviewerId,
                Username = "siti_rahma",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = AccFinanceLeadId,
                EmployeeId = EmpFinanceLeadId,
                RoleId = RoleInterviewerId,
                Username = "ahmad_fauzi",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var acc in accounts)
        {
            acc.PasswordHash = passwordHasher.HashPassword(acc, defaultPassword);
        }

        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedStageTypesAsync(RmsDbContext context)
    {
        if (await context.StageTypes.AnyAsync()) return;

        var stageTypes = new List<StageType>
        {
            new()
            {
                Id = StageTypePsikotesId,
                Name = "Psikotes",
                Description = "Tahap evaluasi profil psikologis, kepribadian, dan gaya kerja kandidat",
                StageCategory = StageCategory.Test,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = StageTypeTechnicalTestId,
                Name = "Technical Test",
                Description = "Uji kompetensi teknis dan keahlian spesifik bidang pekerjaan",
                StageCategory = StageCategory.Test,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = StageTypeLogicalTestId,
                Name = "Logical Test",
                Description = "Uji kemampuan berpikir kritis, logika komputasi, dan pemecahan masalah",
                StageCategory = StageCategory.Test,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = StageTypeInterviewHrId,
                Name = "Interview HR",
                Description = "Wawancara kompetensi umum, riwayat pengalaman, dan kesesuaian budaya organisasi",
                StageCategory = StageCategory.Interview,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = StageTypeInterviewUserId,
                Name = "Interview User",
                Description = "Wawancara teknis mendalam bersama calon atasan dan rekan tim kerja",
                StageCategory = StageCategory.Interview,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.StageTypes.AddRange(stageTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedQuestionBanksAndQuestionsAsync(RmsDbContext context)
    {
        if (await context.QuestionBanks.AnyAsync()) return;

        // 1. Bank Soal Psikotes (DISC) dibuat oleh PsychologyTestSeeder
        // 2. Bank Soal Logical Test
        var bankLogical = new QuestionBank
        {
            Id = QBankLogicalId,
            DepartmentId = DeptTechId,
            StageTypeId = StageTypeLogicalTestId,
            CreatedBy = EmpHrManagerId,
            DurationInMinutes = 20,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Bank Soal Technical Test
        var bankTechnical = new QuestionBank
        {
            Id = QBankTechnicalId,
            DepartmentId = DeptTechId,
            StageTypeId = StageTypeTechnicalTestId,
            CreatedBy = EmpLeadDevId,
            DurationInMinutes = 45,
            CreatedAt = DateTime.UtcNow
        };

        context.QuestionBanks.AddRange(bankLogical, bankTechnical);

        // Tambahkan masing-masing 10 Soal + 4 Opsi Jawaban
        var questions = new List<Question>();
        var options = new List<QuestionOptions>();

        // Logical Test Questions (10)
        AddLogicalQuestions(bankLogical.Id, questions, options);

        // Technical Test Questions (10)
        AddTechnicalQuestions(bankTechnical.Id, questions, options);

        context.Questions.AddRange(questions);
        context.QuestionOptions.AddRange(options);

        await context.SaveChangesAsync();
    }

    #region Question Seeding Helpers

    private static void AddLogicalQuestions(Guid bankId, List<Question> questions, List<QuestionOptions> options)
    {
        var data = new (string QuestionText, string[] Opts, int CorrectIndex)[]
        {
            ("Deret angka: 2, 4, 8, 16, 32, ... Angka berikutnya adalah:",
                ["48", "64", "54", "72"], 1),
            ("Jika SEMUA programmer menyukai kopi, dan Fariz adalah programmer, maka kesimpulannya:",
                ["Fariz menyukai kopi", "Fariz tidak menyukai kopi", "Fariz hanya minum teh", "Tidak dapat disimpulkan"], 0),
            ("Deret angka: 3, 5, 9, 17, 33, ... Pola penambahan deret ini adalah:",
                ["+2, +4, +8, +16", "+2, +3, +4, +5", "+1, +2, +3, +4", "+3, +6, +9, +12"], 0),
            ("Sebuah tangki air terisi penuh dalam 20 menit dengan pipa A, dan 30 menit dengan pipa B. Berapa menit tangki terisi jika kedua pipa dibuka bersama?",
                ["10 menit", "12 menit", "15 menit", "25 menit"], 1),
            ("Jika HARI INI adalah hari Rabu, hari apakah 100 hari setelah hari ini?",
                ["Kamis", "Jumat", "Sabtu", "Minggu"], 1),
            ("Deret huruf: B, D, G, K, P, ... Huruf selanjutnya adalah:",
                ["U", "V", "W", "T"], 1),
            ("Semua mobil butuh bahan bakar. Beberapa kendaraan adalah mobil. Kesimpulannya:",
                ["Semua kendaraan butuh bahan bakar", "Beberapa kendaraan butuh bahan bakar", "Tidak ada kendaraan butuh bahan bakar", "Semua motor butuh bensin"], 1),
            ("Jika 5 mesin dapat membuat 5 produk dalam 5 menit, berapa menit yang dibutuhkan 100 mesin untuk membuat 100 produk?",
                ["100 menit", "5 menit", "50 menit", "1 menit"], 1),
            ("Deret angka: 1, 1, 2, 3, 5, 8, 13, ... Angka selanjutnya dalam deret Fibonacci ini adalah:",
                ["19", "20", "21", "24"], 2),
            ("Jika X > Y dan Y > Z, maka pernyataan yang PASTI BENAR adalah:",
                ["Z > X", "X > Z", "X == Z", "Y == Z"], 1)
        };

        for (int i = 0; i < data.Length; i++)
        {
            var qId = Guid.NewGuid();
            questions.Add(new Question
            {
                Id = qId,
                QuestionBankId = bankId,
                QuestionText = data[i].QuestionText,
                QuestionType = QuestionType.SingleChoice,
                Weight = 1.0m,
                SequenceNumber = i + 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            var labels = new[] { "A", "B", "C", "D" };
            for (int j = 0; j < 4; j++)
            {
                options.Add(new QuestionOptions
                {
                    Id = Guid.NewGuid(),
                    QuestionId = qId,
                    OptionLabel = labels[j],
                    OptionText = data[i].Opts[j],
                    Score = (j == data[i].CorrectIndex) ? 10.0m : 0.0m,
                    SequenceNumber = j + 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static void AddTechnicalQuestions(Guid bankId, List<Question> questions, List<QuestionOptions> options)
    {
        var data = new (string QuestionText, string[] Opts, int CorrectIndex)[]
        {
            ("Di dalam C# dan EF Core, apa fungsi utama dari pemanggilan method AsNoTracking() pada query?",
                ["Mempercepat eksekusi dengan tidak melacak perubahan entity di memory snapshot", "Menghapus data secara permanen", "Mengunci tabel dari transaksi lain", "Membuat transaksi rollback otomatis"], 0),
            ("Apa perbedaan mendasar antara Value Type dan Reference Type dalam platform .NET?",
                ["Value type disimpan di Heap, Reference type disimpan di Stack", "Value type menyimpan data langsung, Reference type menyimpan referensi ke memori Heap", "Keduanya memiliki alokasi memori yang sama persis", "Value type tidak bisa bernilai null sama sekali meskipun nullable"], 1),
            ("Dalam pola arsitektur CQRS, apa prinsip pemisahan utama antara Command dan Query?",
                ["Command mengubah state data tanpa mengembalikan data entity langsung, sedangkan Query hanya membaca data (Idempotent/Read-only)", "Command hanya dipakai untuk delete, Query untuk update", "Command memakai GET, Query memakai POST", "Keduanya menjalankan operasi yang identik"], 0),
            ("Status HTTP Code yang paling tepat dikembalikan ketika validasi payload request klien gagal adalah:",
                ["200 OK", "400 Bad Request / 422 Unprocessable Entity", "500 Internal Server Error", "404 Not Found"], 1),
            ("Dalam Entity Framework Core, DeleteBehavior apakah yang mencegah penghapusan jika masih ada anak entitas yang berelasi?",
                ["DeleteBehavior.Cascade", "DeleteBehavior.Restrict", "DeleteBehavior.SetNull", "DeleteBehavior.ClientCascade"], 1),
            ("Apa keuntungan menggunakan Guid Version 7 (Guid.CreateVersion7()) dibandingkan Guid v4 (Guid.NewGuid())?",
                ["Menghemat ukuran byte menjadi 4 byte", "Mengandung komponen timestamp urut (time-ordered) yang meminimalkan fragmentasi indeks B-Tree database", "Tidak bisa diduplikasi di jaringan internet", "Dapat dienkripsi dengan SHA-256"], 1),
            ("Dalam arsitektur ASP.NET Core, apa fungsi dari MediatR IPipelineBehavior?",
                ["Menjadi middleware sebelum request masuk ke controller", "Mencegat (interception) eksekusi request handler untuk logging, validasi, atau transaksional (Cross-Cutting Concerns)", "Mengatur routing URL", "Menghubungkan langsung controller ke basis data"], 1),
            ("Apa yang dimaksud dengan prinsip 'Dependency Inversion Principle' (DIP) pada SOLID?",
                ["Modul tingkat tinggi tidak boleh bergantung pada modul tingkat rendah; keduanya harus bergantung pada abstraksi", "Semua class harus memiliki turunan interface", "Class hanya boleh memiliki satu method", "Database harus dipisahkan menjadi microservices"], 0),
            ("Kapan sebaiknya kita menggunakan index komposit (composite index) pada tabel SQL Server?",
                ["Ketika kolom selalu dicari secara independen satu per satu", "Ketika kueri filter (WHERE) atau JOIN sering melibatkan kombinasi dua kolom atau lebih secara bersamaan", "Hanya pada kolom bertipe teks panjang nvarchar(max)", "Tidak pernah dianjurkan dalam database relasional"], 1),
            ("Di ASP.NET Core, interface manakah yang dianjurkan untuk mocking waktu sistem saat unit test dijalankan?",
                ["DateTime.Now", "TimeProvider / IDateTimeProvider", "Thread.Sleep", "Stopwatch"], 1)
        };

        for (int i = 0; i < data.Length; i++)
        {
            var qId = Guid.NewGuid();
            questions.Add(new Question
            {
                Id = qId,
                QuestionBankId = bankId,
                QuestionText = data[i].QuestionText,
                QuestionType = QuestionType.SingleChoice,
                Weight = 1.0m,
                SequenceNumber = i + 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            var labels = new[] { "A", "B", "C", "D" };
            for (int j = 0; j < 4; j++)
            {
                options.Add(new QuestionOptions
                {
                    Id = Guid.NewGuid(),
                    QuestionId = qId,
                    OptionLabel = labels[j],
                    OptionText = data[i].Opts[j],
                    Score = (j == data[i].CorrectIndex) ? 10.0m : 0.0m,
                    SequenceNumber = j + 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    #endregion

    private static async Task SeedSampleBatchAndStageAsync(RmsDbContext context)
    {
        var now = DateTime.UtcNow;

        // Database development yang lebih lama dapat memiliki master data dengan ID berbeda
        // dari konstanta seeder. Resolve berdasarkan kode/nama agar reset checkpoint tetap valid.
        var techDepartmentId = await context.Departments
            .Where(x => x.Code == "TECH")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (techDepartmentId == Guid.Empty)
            techDepartmentId = DeptTechId;

        var interviewHrStageTypeId = await context.StageTypes
            .Where(x => x.Name == "Interview HR")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (interviewHrStageTypeId == Guid.Empty)
            interviewHrStageTypeId = StageTypeInterviewHrId;

        if (!await context.RecruitmentBatches.AnyAsync(x => x.Id == SampleBatchId || x.BatchCode == "BATCH-2026-TECH"))
        {
            var batch = new RecruitmentBatch
            {
                Id = SampleBatchId,
                DepartmentId = techDepartmentId,
                BatchCode = "BATCH-2026-TECH",
                Name = "Batch 2026 Tech & Engineering",
                Description = "Recruitment batch for Tech team 2026",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(30),
                Status = BatchStatus.Open,
                CreatedBy = EmpHrManagerId,
                CreatedAt = now
            };
            context.RecruitmentBatches.Add(batch);
        }

        if (!await context.BatchStages.AnyAsync(x => x.Id == SampleInterviewStageId))
        {
            var stage = new BatchStage
            {
                Id = SampleInterviewStageId,
                RecruitmentBatchId = SampleBatchId,
                StageTypeId = interviewHrStageTypeId,
                StageName = "Wawancara HR & User",
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(15),
                CreatedAt = now
            };
            context.BatchStages.Add(stage);
        }

        if (!await context.StageEmployees.AnyAsync(x => x.BatchStageId == SampleInterviewStageId && x.EmployeeId == EmpVpEngId))
        {
            context.StageEmployees.Add(new StageEmployee
            {
                Id = SampleStageEmployeeId,
                BatchStageId = SampleInterviewStageId,
                EmployeeId = EmpVpEngId,
                Status = StageEmployeeStatus.Accepted,
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!await context.Candidates.AnyAsync(x => x.Id == SampleCandidateId || x.Email == "john.doe.candidate@example.com"))
        {
            context.Candidates.Add(new Candidate
            {
                Id = SampleCandidateId,
                FullName = "John Doe (Candidate)",
                Email = "john.doe.candidate@example.com",
                Gender = Gender.Male,
                PhoneNumber = "081234567890",
                BirthDate = new DateOnly(2000, 1, 1),
                Education = "S1 Teknik Informatika",
                Institution = "Institut Teknologi Bandung",
                Address = "Jl. Ganesha No. 10, Bandung",
                Gpa = 3.85m,
                ExpectedSalary = 12000000m,
                Cv = "cv_john_doe.pdf",
                CreatedAt = now
            });
        }

        if (!await context.BatchApplications.AnyAsync(x => x.Id == SampleBatchApplicationId || x.CodeApplication == "APP-2026-001"))
        {
            context.BatchApplications.Add(new BatchApplication
            {
                Id = SampleBatchApplicationId,
                CandidateId = SampleCandidateId,
                RecruitmentBatchId = SampleBatchId,
                CodeApplication = "APP-2026-001",
                AppliedAt = now.AddDays(-3),
                Status = ApplicationStatus.InProcess,
                CreatedAt = now
            });
        }

        if (!await context.ApplicationStages.AnyAsync(x => x.Id == SampleApplicationStageId))
        {
            context.ApplicationStages.Add(new ApplicationStage
            {
                Id = SampleApplicationStageId,
                BatchApplicationId = SampleBatchApplicationId,
                BatchStageId = SampleInterviewStageId,
                Status = ApplicationStageStatus.InProgress,
                StartedAt = now.AddDays(-1),
                EndAt = now.AddDays(7),
                CreatedAt = now
            });
        }

        if (!await context.InterviewSessions.AnyAsync(x => x.Id == SampleInterviewSessionId))
        {
            context.InterviewSessions.Add(new InterviewSession
            {
                Id = SampleInterviewSessionId,
                ApplicationStageId = SampleApplicationStageId,
                SessionName = "Wawancara Teknis Kandidat",
                ScheduledStart = now.AddDays(2).Date.AddHours(9),
                ScheduledEnd = now.AddDays(2).Date.AddHours(10),
                IsOnline = true,
                MeetingUrl = "https://meet.google.com/abc-defg-hij",
                Status = InterviewSessionStatus.PendingConfirmation,
                CreatedBy = EmpHrManagerId,
                CreatedAt = now
            });
        }

        if (!await context.SessionInterviews.AnyAsync(x => x.Id == SampleSessionInterviewId))
        {
            context.SessionInterviews.Add(new SessionInterview
            {
                Id = SampleSessionInterviewId,
                InterviewSessionId = SampleInterviewSessionId,
                StageEmployeeId = SampleStageEmployeeId,
                Status = SessionInterviewStatus.InProgress,
                IsLead = true,
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync();
    }
}
