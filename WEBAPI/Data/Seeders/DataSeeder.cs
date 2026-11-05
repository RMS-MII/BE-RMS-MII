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
    public static readonly Guid EmpHrManagerFallbackId = Guid.Parse("01930003-0000-7000-8000-000000000099");
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

    public static async Task SeedAsync(RmsDbContext context, IPasswordHasher<Account> passwordHasher)
    {
        Log.Information("Checking and running idempotent database seeder...");

        await SeedDepartmentsAsync(context);
        await SeedRolesAsync(context);
        await SeedEmployeesAsync(context);
        await SeedAccountsAsync(context, passwordHasher);
        await SeedStageTypesAsync(context);
        await SeedQuestionBanksAndQuestionsAsync(context);
        await SeedSampleBatchAndStageAsync(context);

        Log.Information("Database seeder completed successfully.");
    }

    /// <summary>
    /// Checkpoint reset: membersihkan semua data pengujian development (transaksional & testing)
    /// dan mengembalikan data sample dan master ke status pristine (checkpoint awal).
    /// </summary>
    public static async Task ResetToCheckpointAsync(RmsDbContext context, IPasswordHasher<Account> passwordHasher)
    {
        Log.Information("Resetting database to seed checkpoint...");

        // 1. Bersihkan data transaksional dan hasil pengujian
        await context.CandidateAnswers.ExecuteDeleteAsync();
        await context.EmailLogs.ExecuteDeleteAsync();
        await context.InterviewAssessments.ExecuteDeleteAsync();
        await context.RescheduleRequests.ExecuteDeleteAsync();

        // Hapus sesi wawancara selain sample awal
        await context.SessionInterviews
            .Where(x => x.Id != SampleSessionInterviewId)
            .ExecuteDeleteAsync();

        await context.InterviewSessions
            .Where(x => x.Id != SampleInterviewSessionId)
            .ExecuteDeleteAsync();

        // Hapus penugasan pewawancara selain sample awal
        await context.StageEmployees
            .Where(x => x.Id != SampleStageEmployeeId)
            .ExecuteDeleteAsync();

        // Hapus tahapan aplikasi & aplikasi selain sample awal
        await context.ApplicationStages
            .Where(x => x.Id != SampleApplicationStageId)
            .ExecuteDeleteAsync();

        await context.BatchApplications
            .Where(x => x.Id != SampleBatchApplicationId)
            .ExecuteDeleteAsync();

        // Hapus kandidat tambahan selain sample awal
        await context.Candidates
            .Where(x => x.Id != SampleCandidateId)
            .ExecuteDeleteAsync();

        // Hapus tahapan batch & batch tambahan selain sample awal
        await context.BatchStages
            .Where(x => x.Id != SampleInterviewStageId)
            .ExecuteDeleteAsync();

        await context.RecruitmentBatches
            .Where(x => x.Id != SampleBatchId)
            .ExecuteDeleteAsync();

        // 2. Kembalikan data sample ke kondisi awal (pristine checkpoint)
        var now = DateTime.UtcNow;

        var sampleSession = await context.InterviewSessions.FirstOrDefaultAsync(x => x.Id == SampleInterviewSessionId);
        if (sampleSession is not null)
        {
            sampleSession.SessionName = "Wawancara Teknis Kandidat";
            sampleSession.ScheduledStart = now.AddDays(2).Date.AddHours(9);
            sampleSession.ScheduledEnd = now.AddDays(2).Date.AddHours(10);
            sampleSession.IsOnline = true;
            sampleSession.MeetingUrl = "https://meet.google.com/abc-defg-hij";
            sampleSession.Location = null;
            sampleSession.Status = InterviewSessionStatus.PendingConfirmation;
            sampleSession.FileSubmission = null;
            sampleSession.EndedSubmissionAt = null;
            sampleSession.UpdatedAt = null;
        }

        var sampleSessionInterview = await context.SessionInterviews.FirstOrDefaultAsync(x => x.Id == SampleSessionInterviewId);
        if (sampleSessionInterview is not null)
        {
            sampleSessionInterview.Status = SessionInterviewStatus.InProgress;
            sampleSessionInterview.UpdatedAt = null;
        }

        var sampleStageEmp = await context.StageEmployees.FirstOrDefaultAsync(x => x.Id == SampleStageEmployeeId);
        if (sampleStageEmp is not null)
        {
            sampleStageEmp.Status = StageEmployeeStatus.Accepted;
            sampleStageEmp.IsActive = true;
            sampleStageEmp.Note = null;
            sampleStageEmp.UpdatedAt = null;
        }

        var sampleAppStage = await context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == SampleApplicationStageId);
        if (sampleAppStage is not null)
        {
            sampleAppStage.Status = ApplicationStageStatus.InProgress;
            sampleAppStage.StartedAt = now.AddDays(-1);
            sampleAppStage.EndAt = now.AddDays(7);
            sampleAppStage.CompletedAt = null;
            sampleAppStage.Notes = null;
            sampleAppStage.IsAttempts = false;
            sampleAppStage.DurationTest = null;
            sampleAppStage.TotalScore = null;
            sampleAppStage.UpdatedAt = null;
        }

        var sampleApp = await context.BatchApplications.FirstOrDefaultAsync(x => x.Id == SampleBatchApplicationId);
        if (sampleApp is not null)
        {
            sampleApp.Status = ApplicationStatus.InProcess;
            sampleApp.FinalResultStatus = null;
            sampleApp.OfferingLetterStatus = OfferingLetterStatus.Pending;
            sampleApp.FileOfferingLetter = string.Empty;
            sampleApp.Notes = string.Empty;
            sampleApp.UpdatedAt = null;
        }

        var sampleCandidate = await context.Candidates.FirstOrDefaultAsync(x => x.Id == SampleCandidateId);
        if (sampleCandidate is not null)
        {
            sampleCandidate.FullName = "John Doe (Candidate)";
            sampleCandidate.Email = "john.doe.candidate@example.com";
            sampleCandidate.Gender = Gender.Male;
            sampleCandidate.PhoneNumber = "081200000001";
            sampleCandidate.BirthDate = new DateOnly(2000, 1, 1);
            sampleCandidate.Education = "S1 Teknik Informatika";
            sampleCandidate.Institution = "Institut Teknologi Bandung";
            sampleCandidate.Address = "Jl. Ganesha No. 10, Bandung";
            sampleCandidate.Gpa = 3.85m;
            sampleCandidate.ExpectedSalary = 12000000m;
            sampleCandidate.Cv = "cv_john_doe.pdf";
            sampleCandidate.UpdatedAt = null;
        }

        var sampleStage = await context.BatchStages.FirstOrDefaultAsync(x => x.Id == SampleInterviewStageId);
        if (sampleStage is not null)
        {
            sampleStage.StageName = "Wawancara HR & User";
            sampleStage.IsActive = true;
            sampleStage.StartDate = now.AddDays(-2);
            sampleStage.EndDate = now.AddDays(15);
            sampleStage.UpdatedAt = null;
        }

        var sampleBatch = await context.RecruitmentBatches.FirstOrDefaultAsync(x => x.Id == SampleBatchId);
        if (sampleBatch is not null)
        {
            sampleBatch.Name = "Batch 2026 Tech & Engineering";
            sampleBatch.Status = BatchStatus.Open;
            sampleBatch.StartDate = now.AddDays(-5);
            sampleBatch.EndDate = now.AddDays(30);
            sampleBatch.UpdatedAt = null;
        }

        await context.SaveChangesAsync();

        // 3. Pastikan kelengkapan master data
        await SeedAsync(context, passwordHasher);

        Log.Information("Database checkpoint restored successfully.");
    }

    private static async Task SeedDepartmentsAsync(RmsDbContext context)
    {
        var techId = await EnsureDepartmentAsync(
            context,
            DeptTechId,
            "TECH",
            "Technology",
            "Divisi Teknologi dan Rekayasa Sistem Perusahaan");

        await EnsureDepartmentAsync(
            context,
            DeptHrId,
            "HR",
            "Human Resources",
            "Departemen Manajemen Sumber Daya Manusia dan Rekrutmen");

        await EnsureDepartmentAsync(
            context,
            DeptFinanceId,
            "FIN",
            "Finance",
            "Departemen Keuangan dan Akuntansi");

        var softwareEngineeringId = await context.Departments
            .Where(department => department.Code == "SE" || department.Code == "ENG")
            .Select(department => (Guid?)department.Id)
            .FirstOrDefaultAsync();

        if (softwareEngineeringId is null)
        {
            await EnsureDepartmentAsync(
                context,
                DeptSoftwareEngId,
                "SE",
                "Software Engineering",
                "Departemen Pengembangan Perangkat Lunak dan Aplikasi",
                techId);
        }

        await EnsureDepartmentAsync(
            context,
            DeptQaId,
            "QA",
            "Quality Assurance",
            "Departemen Penjaminan Kualitas dan Pengujian Sistem",
            techId);
    }

    private static async Task SeedRolesAsync(RmsDbContext context)
    {
        await EnsureRoleAsync(
            context,
            RoleHrId,
            "HR",
            "Hak akses administrator HR dan pengelola rekrutmen");

        await EnsureRoleAsync(
            context,
            RoleInterviewerId,
            "Interviewer",
            "Hak akses pewawancara teknis dan user");
    }

    private static async Task SeedEmployeesAsync(RmsDbContext context)
    {
        var hrDepartmentId = await GetDepartmentIdAsync(context, "HR");
        var techDepartmentId = await GetDepartmentIdAsync(context, "TECH");
        var softwareEngineeringDepartmentId = await GetSoftwareEngineeringDepartmentIdAsync(context);
        var qaDepartmentId = await GetDepartmentIdAsync(context, "QA");
        var financeDepartmentId = await GetDepartmentIdAsync(context, "FIN");

        await EnsureEmployeeAsync(
            context,
            EmpHrManagerId,
            "EMP-HR-001",
            "Jane Doe",
            "jane.doe@rms.local",
            Gender.Female,
            "HR Recruitment Manager",
            hrDepartmentId);

        var vpEngineeringId = await EnsureEmployeeAsync(
            context,
            EmpVpEngId,
            "EMP-TECH-001",
            "Budi Santoso",
            "budi.santoso@rms.local",
            Gender.Male,
            "VP of Engineering",
            techDepartmentId);

        await EnsureEmployeeAsync(
            context,
            EmpLeadDevId,
            "EMP-SE-001",
            "Fariz Irza",
            "fariz.irza@rms.local",
            Gender.Male,
            "Lead Software Engineer",
            softwareEngineeringDepartmentId,
            vpEngineeringId);

        await EnsureEmployeeAsync(
            context,
            EmpQaLeadId,
            "EMP-QA-001",
            "Siti Rahma",
            "siti.rahma@rms.local",
            Gender.Female,
            "QA Lead",
            qaDepartmentId,
            vpEngineeringId);

        await EnsureEmployeeAsync(
            context,
            EmpFinanceLeadId,
            "EMP-FIN-001",
            "Ahmad Fauzi",
            "ahmad.fauzi@rms.local",
            Gender.Male,
            "Finance Lead",
            financeDepartmentId);
    }

    private static async Task SeedAccountsAsync(RmsDbContext context, IPasswordHasher<Account> passwordHasher)
    {
        var hrRoleId = await GetRoleIdAsync(context, "HR");
        var interviewerRoleId = await GetRoleIdAsync(context, "Interviewer");

        var hrManagerEmployeeNumber = "EMP-HR-001";
        var hrManagerEmployeeId = await GetEmployeeIdAsync(context, hrManagerEmployeeNumber);
        var hrManagerAccountExists = await context.Accounts
            .AnyAsync(account => account.Username == "hr_manager");
        var hrManagerEmployeeHasAccount = await context.Accounts
            .AnyAsync(account => account.EmployeeId == hrManagerEmployeeId);

        if (!hrManagerAccountExists && hrManagerEmployeeHasAccount)
        {
            var hrDepartmentId = await GetDepartmentIdAsync(context, "HR");

            await EnsureEmployeeAsync(
                context,
                EmpHrManagerFallbackId,
                "EMP-HR-900",
                "HR Manager (Seeder)",
                "hr.manager.seed@rms.local",
                Gender.Female,
                "HR Recruitment Manager",
                hrDepartmentId);

            hrManagerEmployeeNumber = "EMP-HR-900";
        }

        await EnsureAccountAsync(context, passwordHasher, AccHrManagerId, "hr_manager", hrManagerEmployeeNumber, hrRoleId);
        await EnsureAccountAsync(context, passwordHasher, AccVpEngId, "budi_santoso", "EMP-TECH-001", interviewerRoleId);
        await EnsureAccountAsync(context, passwordHasher, AccLeadDevId, "fariz_irza", "EMP-SE-001", interviewerRoleId);
        await EnsureAccountAsync(context, passwordHasher, AccQaLeadId, "siti_rahma", "EMP-QA-001", interviewerRoleId);
        await EnsureAccountAsync(context, passwordHasher, AccFinanceLeadId, "ahmad_fauzi", "EMP-FIN-001", interviewerRoleId);
    }

    private static async Task SeedStageTypesAsync(RmsDbContext context)
    {
        await EnsureStageTypeAsync(
            context,
            StageTypePsikotesId,
            "Psikotes",
            "Tahap evaluasi profil psikologis, kepribadian, dan gaya kerja kandidat",
            StageCategory.Test);

        await EnsureStageTypeAsync(
            context,
            StageTypeInterviewHrId,
            "Interview HR",
            "Wawancara kompetensi umum, riwayat pengalaman, dan kesesuaian budaya organisasi",
            StageCategory.Interview);

        await EnsureStageTypeAsync(
            context,
            StageTypeInterviewUserId,
            "Interview User",
            "Wawancara teknis mendalam bersama calon atasan dan rekan tim kerja",
            StageCategory.Interview);

        var hasCombinedTestStage = await context.StageTypes
            .AnyAsync(stageType => stageType.Name == "Technical & Logical Test");

        if (!hasCombinedTestStage)
        {
            await EnsureStageTypeAsync(
                context,
                StageTypeTechnicalTestId,
                "Technical Test",
                "Uji kompetensi teknis dan keahlian spesifik bidang pekerjaan",
                StageCategory.Test);

            await EnsureStageTypeAsync(
                context,
                StageTypeLogicalTestId,
                "Logical Test",
                "Uji kemampuan berpikir kritis, logika komputasi, dan pemecahan masalah",
                StageCategory.Test);
        }
    }

    private static async Task SeedQuestionBanksAndQuestionsAsync(RmsDbContext context)
    {
        if (await context.QuestionBanks.AnyAsync()) return;

        var hrDepartmentId = await GetDepartmentIdAsync(context, "HR");
        var techDepartmentId = await GetDepartmentIdAsync(context, "TECH");
        var softwareEngineeringDepartmentId = await GetSoftwareEngineeringDepartmentIdAsync(context);
        var hrManagerId = await GetEmployeeIdAsync(context, "EMP-HR-001");
        var leadSoftwareEngineerId = await GetEmployeeIdAsync(context, "EMP-SE-001");
        var psikotesStageTypeId = await GetStageTypeIdAsync(context, "Psikotes");

        var combinedTestStageTypeId = await FindStageTypeIdAsync(context, "Technical & Logical Test");
        var logicalStageTypeId = combinedTestStageTypeId
            ?? await GetStageTypeIdAsync(context, "Logical Test");
        var technicalStageTypeId = combinedTestStageTypeId
            ?? await GetStageTypeIdAsync(context, "Technical Test");

        // 1. Bank Soal Psikotes
        var bankPsikotes = new QuestionBank
        {
            Id = QBankPsikotesId,
            DepartmentId = hrDepartmentId,
            StageTypeId = psikotesStageTypeId,
            CreatedBy = hrManagerId,
            CreatedAt = DateTime.UtcNow
        };

        // 2. Bank Soal Logical Test
        var bankLogical = new QuestionBank
        {
            Id = QBankLogicalId,
            DepartmentId = techDepartmentId,
            StageTypeId = logicalStageTypeId,
            CreatedBy = hrManagerId,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Bank Soal Technical Test (digabung jika master data memakai Technical & Logical Test)
        QuestionBank? bankTechnical = null;
        if (combinedTestStageTypeId is null)
        {
            bankTechnical = new QuestionBank
            {
                Id = QBankTechnicalId,
                DepartmentId = softwareEngineeringDepartmentId,
                StageTypeId = technicalStageTypeId,
                CreatedBy = leadSoftwareEngineerId,
                CreatedAt = DateTime.UtcNow
            };
        }

        var questionBanks = new List<QuestionBank> { bankPsikotes, bankLogical };
        if (bankTechnical is not null)
            questionBanks.Add(bankTechnical);

        context.QuestionBanks.AddRange(questionBanks);

        // Tambahkan masing-masing 10 Soal + 4 Opsi Jawaban
        var questions = new List<Question>();
        var options = new List<QuestionOptions>();

        // Psikotes Questions (10)
        AddPsikotesQuestions(bankPsikotes.Id, questions, options);

        // Logical Test Questions (10)
        AddLogicalQuestions(bankLogical.Id, questions, options);

        // Technical Test Questions (10)
        AddTechnicalQuestions((bankTechnical ?? bankLogical).Id, questions, options);

        context.Questions.AddRange(questions);
        context.QuestionOptions.AddRange(options);

        await context.SaveChangesAsync();
    }

    #region Question Seeding Helpers

    private static void AddPsikotesQuestions(Guid bankId, List<Question> questions, List<QuestionOptions> options)
    {
        var data = new (string QuestionText, string[] Opts, int CorrectIndex)[]
        {
            ("Saat menghadapi perubahan mendadak dalam prioritas tugas, bagaimana sikap Anda?",
                ["Menolak karena mengganggu rencana awal", "Beradaptasi dengan menyusun ulang jadwal prioritas", "Menunggu instruksi lebih lanjut tanpa mengambil inisiatif", "Mengeluh kepada rekan kerja"], 1),
            ("Ketika rekan satu tim mengalami kendala dan proyek terancam terlambat, apa yang Anda lakukan?",
                ["Menyelesaikan tugas sendiri saja", "Menyalahkan rekan tersebut ke atasan", "Menawarkan bantuan dan mendiskusikan solusi bersama", "Membiarkan karena itu bukan tanggung jawab saya"], 2),
            ("Bagaimana cara Anda mengelola stres saat beban kerja meningkat pesat?",
                ["Mengabaikan deadline dan bersantai", "Membuat skala prioritas dan beristirahat berkala secara teratur", "Bekerja tanpa tidur hingga jatuh sakit", "Menyerahkan tugas ke orang lain"], 1),
            ("Jika Anda menemukan kesalahan minor pada pekerjaan rekan yang sudah diserahkan ke klien:",
                ["Mendiamkannya agar tidak timbul masalah", "Menginfokan rekan tersebut secara privat dan membantunya memperbaikinya", "Menertawakannya di depan umum", "Segera melaporkan ke direktur"], 1),
            ("Sikap yang paling tepat saat menerima umpan balik kritis yang membangun dari atasan adalah:",
                ["Menerima dengan objektif dan menjadikannya rencana perbaikan diri", "Merasa tersinggung dan membela diri secara agresif", "Mengabaikan masukan tersebut", "Langsung mengajukan resign"], 0),
            ("Ketika Anda dihadapkan pada dua pilihan penting dengan data yang terbatas, Anda:",
                ["Menunda keputusan tanpa batas waktu", "Mengumpulkan fakta kunci yang relevan dan mengambil keputusan berbasis analisis risiko", "Memilih secara acak dengan melempar koin", "Meminta orang lain memutuskan agar terhindar dari tanggung jawab"], 1),
            ("Menurut Anda, kunci utama keberhasilan kolaborasi tim lintas fungsi adalah:",
                ["Menonjolkan kehebatan individu sendiri", "Komunikasi terbuka, rasa saling percaya, dan tujuan yang selaras", "Bekerja secara terisolasi tanpa interaksi", "Mengikuti hanya apa yang diinginkan pihak yang paling vokal"], 1),
            ("Jika Anda diberi tugas baru yang belum pernah Anda kerjakan sebelumnya:",
                ["Menolak tugas karena bukan bidang keahlian", "Mempelajari dasar-dasarnya, bertanya kepada ahli, dan mengerjakannya bertahap", "Mengerjakan asal-asalan tanpa panduan", "Menunggu hingga ada yang mengambil alih"], 1),
            ("Bagaimana Anda menyikapi perbedaan pendapat yang tajam dalam rapat diskusi tim?",
                ["Memaksakan argumen sendiri sampai diterima", "Mendengarkan perspektif rekan lain dan mencari titik temu yang solutif", "Meninggalkan ruangan rapat dengan marah", "Diam saja meski tahu rencana tersebut cacat"], 1),
            ("Apa yang Anda lakukan saat target pribadi Anda selesai lebih awal dari jadwal yang ditentukan?",
                ["Langsung pulang tanpa koordinasi", "Mengecek kembali kualitas hasil kerja dan menawarkan bantuan ke tim jika dibutuhkan", "Bermain game di meja kerja", "Menyembunyikan bahwa tugas sudah selesai"], 1)
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
        var techDepartmentId = await GetDepartmentIdAsync(context, "TECH");
        var hrManagerId = await GetEmployeeIdAsync(context, "EMP-HR-001");
        var vpEngineeringId = await GetEmployeeIdAsync(context, "EMP-TECH-001");
        var interviewHrStageTypeId = await GetStageTypeIdAsync(context, "Interview HR");
        const string sampleBatchCode = "BATCH-2026-TECH";
        const string sampleStageName = "Wawancara HR & User";
        const string sampleSessionName = "Wawancara Teknis Kandidat";

        var batch = await context.RecruitmentBatches
            .FirstOrDefaultAsync(x => x.Id == SampleBatchId || x.BatchCode == sampleBatchCode);

        if (batch is null)
        {
            batch = new RecruitmentBatch
            {
                Id = SampleBatchId,
                DepartmentId = techDepartmentId,
                BatchCode = sampleBatchCode,
                Name = "Batch 2026 Tech & Engineering",
                Description = "Recruitment batch for Tech team 2026",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(30),
                Status = BatchStatus.Open,
                CreatedBy = hrManagerId,
                CreatedAt = now
            };

            context.RecruitmentBatches.Add(batch);
            await context.SaveChangesAsync();
        }

        var stage = await context.BatchStages
            .FirstOrDefaultAsync(x => x.Id == SampleInterviewStageId
                || (x.RecruitmentBatchId == batch.Id && x.StageName == sampleStageName));

        if (stage is null)
        {
            stage = new BatchStage
            {
                Id = SampleInterviewStageId,
                RecruitmentBatchId = batch.Id,
                StageTypeId = interviewHrStageTypeId,
                StageName = sampleStageName,
                IsActive = true,
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(15),
                CreatedAt = now
            };

            context.BatchStages.Add(stage);
            await context.SaveChangesAsync();
        }

        var stageEmployee = await context.StageEmployees
            .FirstOrDefaultAsync(x => x.BatchStageId == stage.Id && x.EmployeeId == vpEngineeringId);

        if (stageEmployee is null)
        {
            stageEmployee = new StageEmployee
            {
                Id = SampleStageEmployeeId,
                BatchStageId = stage.Id,
                EmployeeId = vpEngineeringId,
                Status = StageEmployeeStatus.Accepted,
                IsActive = true,
                CreatedAt = now
            };

            context.StageEmployees.Add(stageEmployee);
            await context.SaveChangesAsync();
        }

        var candidate = await context.Candidates
            .FirstOrDefaultAsync(x => x.Id == SampleCandidateId || x.Email == "john.doe.candidate@example.com");

        if (candidate is null)
        {
            candidate = new Candidate
            {
                Id = SampleCandidateId,
                FullName = "John Doe (Candidate)",
                Email = "john.doe.candidate@example.com",
                Gender = Gender.Male,
                PhoneNumber = "081200000001",
                BirthDate = new DateOnly(2000, 1, 1),
                Education = "S1 Teknik Informatika",
                Institution = "Institut Teknologi Bandung",
                Address = "Jl. Ganesha No. 10, Bandung",
                Gpa = 3.85m,
                ExpectedSalary = 12000000m,
                Cv = "cv_john_doe.pdf",
                CreatedAt = now
            };

            context.Candidates.Add(candidate);
            await context.SaveChangesAsync();
        }

        var application = await context.BatchApplications
            .FirstOrDefaultAsync(x => x.Id == SampleBatchApplicationId || x.CodeApplication == "APP-2026-001");

        if (application is null)
        {
            application = new BatchApplication
            {
                Id = SampleBatchApplicationId,
                CandidateId = candidate.Id,
                RecruitmentBatchId = batch.Id,
                CodeApplication = "APP-2026-001",
                AppliedAt = now.AddDays(-3),
                Status = ApplicationStatus.InProcess,
                CreatedAt = now
            };

            context.BatchApplications.Add(application);
            await context.SaveChangesAsync();
        }

        var applicationStage = await context.ApplicationStages
            .FirstOrDefaultAsync(x => x.Id == SampleApplicationStageId
                || (x.BatchApplicationId == application.Id && x.BatchStageId == stage.Id));

        if (applicationStage is null)
        {
            applicationStage = new ApplicationStage
            {
                Id = SampleApplicationStageId,
                BatchApplicationId = application.Id,
                BatchStageId = stage.Id,
                Status = ApplicationStageStatus.InProgress,
                StartedAt = now.AddDays(-1),
                EndAt = now.AddDays(7),
                CreatedAt = now
            };

            context.ApplicationStages.Add(applicationStage);
            await context.SaveChangesAsync();
        }

        var session = await context.InterviewSessions
            .FirstOrDefaultAsync(x => x.Id == SampleInterviewSessionId
                || (x.ApplicationStageId == applicationStage.Id && x.SessionName == sampleSessionName));

        if (session is null)
        {
            session = new InterviewSession
            {
                Id = SampleInterviewSessionId,
                ApplicationStageId = applicationStage.Id,
                SessionName = sampleSessionName,
                ScheduledStart = now.AddDays(2).Date.AddHours(9),
                ScheduledEnd = now.AddDays(2).Date.AddHours(10),
                IsOnline = true,
                MeetingUrl = "https://meet.google.com/abc-defg-hij",
                Status = InterviewSessionStatus.PendingConfirmation,
                CreatedBy = hrManagerId,
                CreatedAt = now
            };

            context.InterviewSessions.Add(session);
            await context.SaveChangesAsync();
        }

        var sessionInterview = await context.SessionInterviews
            .FirstOrDefaultAsync(x => x.Id == SampleSessionInterviewId
                || (x.InterviewSessionId == session.Id && x.StageEmployeeId == stageEmployee.Id));

        if (sessionInterview is null)
        {
            context.SessionInterviews.Add(new SessionInterview
            {
                Id = SampleSessionInterviewId,
                InterviewSessionId = session.Id,
                StageEmployeeId = stageEmployee.Id,
                Status = SessionInterviewStatus.InProgress,
                CreatedAt = now
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<Guid> EnsureDepartmentAsync(
        RmsDbContext context,
        Guid seedId,
        string code,
        string name,
        string description,
        Guid? parentDepartmentId = null)
    {
        var existing = await context.Departments
            .FirstOrDefaultAsync(department => department.Code == code || department.Id == seedId);

        if (existing is not null)
            return existing.Id;

        var department = new Department
        {
            Id = seedId,
            Code = code,
            Name = name,
            Description = description,
            ParentDepartmentId = parentDepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync();

        return department.Id;
    }

    private static async Task<Guid> EnsureRoleAsync(
        RmsDbContext context,
        Guid seedId,
        string name,
        string description)
    {
        var existing = await context.Roles
            .FirstOrDefaultAsync(role => role.Name == name || role.Id == seedId);

        if (existing is not null)
            return existing.Id;

        var role = new Role
        {
            Id = seedId,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.Add(role);
        await context.SaveChangesAsync();

        return role.Id;
    }

    private static async Task<Guid> EnsureEmployeeAsync(
        RmsDbContext context,
        Guid seedId,
        string employeeNumber,
        string fullName,
        string email,
        Gender gender,
        string jobTitle,
        Guid departmentId,
        Guid? managerId = null)
    {
        var existing = await context.Employees
            .FirstOrDefaultAsync(employee => employee.EmployeeNumber == employeeNumber
                || employee.Email == email
                || employee.Id == seedId);

        if (existing is not null)
            return existing.Id;

        var employee = new Employee
        {
            Id = seedId,
            DepartmentId = departmentId,
            ManagerId = managerId,
            EmployeeNumber = employeeNumber,
            FullName = fullName,
            Email = email,
            Gender = gender,
            JobTitle = jobTitle,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return employee.Id;
    }

    private static async Task EnsureAccountAsync(
        RmsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        Guid seedId,
        string username,
        string employeeNumber,
        Guid roleId)
    {
        var employeeId = await GetEmployeeIdAsync(context, employeeNumber);
        var existing = await context.Accounts
            .FirstOrDefaultAsync(account => account.Username == username
                || account.Id == seedId
                || account.EmployeeId == employeeId);

        if (existing is not null)
            return;

        var account = new Account
        {
            Id = seedId,
            EmployeeId = employeeId,
            RoleId = roleId,
            Username = username,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        account.PasswordHash = passwordHasher.HashPassword(account, "Password123!");
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
    }

    private static async Task<Guid> EnsureStageTypeAsync(
        RmsDbContext context,
        Guid seedId,
        string name,
        string description,
        StageCategory stageCategory)
    {
        var existing = await context.StageTypes
            .FirstOrDefaultAsync(stageType => stageType.Name == name || stageType.Id == seedId);

        if (existing is not null)
            return existing.Id;

        var stageType = new StageType
        {
            Id = seedId,
            Name = name,
            Description = description,
            StageCategory = stageCategory,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.StageTypes.Add(stageType);
        await context.SaveChangesAsync();

        return stageType.Id;
    }

    private static async Task<Guid> GetDepartmentIdAsync(RmsDbContext context, params string[] codes)
    {
        var departmentId = await context.Departments
            .Where(department => codes.Contains(department.Code))
            .Select(department => (Guid?)department.Id)
            .FirstOrDefaultAsync();

        if (departmentId is null)
            throw new InvalidOperationException(
                $"Department with code '{string.Join("' or '", codes)}' was not found.");

        return departmentId.Value;
    }

    private static async Task<Guid> GetSoftwareEngineeringDepartmentIdAsync(RmsDbContext context)
        => await GetDepartmentIdAsync(context, "SE", "ENG");

    private static async Task<Guid> GetRoleIdAsync(RmsDbContext context, string name)
    {
        var roleId = await context.Roles
            .Where(role => role.Name == name)
            .Select(role => (Guid?)role.Id)
            .FirstOrDefaultAsync();

        if (roleId is null)
            throw new InvalidOperationException($"Role '{name}' was not found.");

        return roleId.Value;
    }

    private static async Task<Guid> GetEmployeeIdAsync(RmsDbContext context, string employeeNumber)
    {
        var employeeId = await context.Employees
            .Where(employee => employee.EmployeeNumber == employeeNumber)
            .Select(employee => (Guid?)employee.Id)
            .FirstOrDefaultAsync();

        if (employeeId is null)
            throw new InvalidOperationException($"Employee '{employeeNumber}' was not found.");

        return employeeId.Value;
    }

    private static async Task<Guid> GetStageTypeIdAsync(RmsDbContext context, string name)
    {
        var stageTypeId = await FindStageTypeIdAsync(context, name);

        if (stageTypeId is null)
            throw new InvalidOperationException($"Stage type '{name}' was not found.");

        return stageTypeId.Value;
    }

    private static async Task<Guid?> FindStageTypeIdAsync(RmsDbContext context, string name)
        => await context.StageTypes
            .Where(stageType => stageType.Name == name)
            .Select(stageType => (Guid?)stageType.Id)
            .FirstOrDefaultAsync();
}
