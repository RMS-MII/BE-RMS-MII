using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Seeders;

/// <summary>
/// Seeder data volume untuk pengujian performa aplikasi.
/// Semua data diberi marker (batch PERF-*, kandidat perf.cand.*, pegawai perf.emp.*)
/// sehingga dapat dibersihkan tanpa menyentuh data manual.
/// </summary>
public static class PerformanceDataSeeder
{
    private const string BatchPrefix = "PERF-";
    private const string CandidateEmailPrefix = "perf.cand.";
    private const string CandidateEmailDomain = "@example.test";
    private const string EmployeeEmailPrefix = "perf.emp.";
    private const string EmployeeEmailDomain = "@rms.local";
    private const string DefaultPassword = "Password123!";

    private static readonly string[] RescheduleReasons =
    [
        "Bentrok jadwal dengan kegiatan lain",
        "Kondisi kesehatan tidak memungkinkan",
        "Perjalanan dinas mendadak",
        "Kebutuhan tim yang mendesak",
        "Jadwal bertabrakan dengan wawancara lain"
    ];

    private static readonly (string Name, Type Type)[] CandidateColumns =
    [
        ("Id", typeof(Guid)), ("IsInTalentPool", typeof(bool)), ("FullName", typeof(string)),
        ("Gender", typeof(int)), ("Email", typeof(string)), ("PhoneNumber", typeof(string)),
        ("BirthDate", typeof(DateTime)), ("Cv", typeof(string)), ("Portfolio", typeof(string)),
        ("Education", typeof(string)), ("Institution", typeof(string)), ("Address", typeof(string)),
        ("ExpectedSalary", typeof(decimal)), ("Gpa", typeof(decimal)), ("CreatedAt", typeof(DateTime)),
        ("UpdatedAt", typeof(DateTime))
    ];

    private static readonly (string Name, Type Type)[] ApplicationColumns =
    [
        ("Id", typeof(Guid)), ("CodeApplication", typeof(string)), ("AppliedAt", typeof(DateTime)),
        ("FinalResultStatus", typeof(int)), ("Status", typeof(int)), ("Notes", typeof(string)),
        ("OfferingLetterStatus", typeof(int)), ("FileOfferingLetter", typeof(string)),
        ("CandidateId", typeof(Guid)), ("RecruitmentBatchId", typeof(Guid)),
        ("ExpectedSalary", typeof(decimal)), ("IsStandalone", typeof(bool)),
        ("CreatedAt", typeof(DateTime)), ("UpdatedAt", typeof(DateTime))
    ];

    private static readonly (string Name, Type Type)[] ApplicationStageColumns =
    [
        ("Id", typeof(Guid)), ("Status", typeof(int)), ("TotalScore", typeof(decimal)),
        ("StartedAt", typeof(DateTime)), ("EndAt", typeof(DateTime)),
        ("AttemptStartedAt", typeof(DateTime)), ("CompletedAt", typeof(DateTime)),
        ("Notes", typeof(string)), ("IsAttempts", typeof(bool)), ("DurationTest", typeof(TimeSpan)),
        ("BatchApplicationId", typeof(Guid)), ("BatchStageId", typeof(Guid)),
        ("CreatedAt", typeof(DateTime)), ("UpdatedAt", typeof(DateTime))
    ];

    private static readonly (string Name, Type Type)[] CandidateAnswerColumns =
    [
        ("Id", typeof(Guid)), ("ApplicationStageId", typeof(Guid)), ("DiscType", typeof(int)),
        ("QuestionText", typeof(string)), ("QuestionOptionText", typeof(string)),
        ("Score", typeof(decimal)), ("CreatedAt", typeof(DateTime)), ("UpdatedAt", typeof(DateTime))
    ];

    private static readonly (string Name, Type Type)[] EmailLogColumns =
    [
        ("Id", typeof(Guid)), ("SenderEmail", typeof(string)), ("RecipientEmail", typeof(string)),
        ("Type", typeof(int)), ("Subject", typeof(string)), ("Body", typeof(string)),
        ("HtmlBody", typeof(string)), ("Status", typeof(int)), ("AttemptedAt", typeof(DateTime)),
        ("SentAt", typeof(DateTime)), ("ErrorMessage", typeof(string))
    ];

    public static async Task SeedAsync(
        RmsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        PerformanceSeedOptions options,
        ILogger logger,
        CancellationToken ct = default)
    {
        options.Normalize();

        var now = DateTime.UtcNow;
        var random = new Random(options.RandomSeed);
        var totalStopwatch = Stopwatch.StartNew();

        if (options.ResetBeforeSeed)
        {
            var clearStopwatch = Stopwatch.StartNew();
            await ClearAsync(context, ct);
            logger.LogInformation("[PerfSeed] Bersihkan data perf lama dalam {Elapsed:n1} dtk.",
                clearStopwatch.Elapsed.TotalSeconds);
        }
        else if (await context.RecruitmentBatches
                     .AnyAsync(batch => batch.BatchCode.StartsWith(BatchPrefix), ct))
        {
            logger.LogWarning(
                "[PerfSeed] Dilewati: data perf sudah ada dan ResetBeforeSeed=false. " +
                "Aktifkan ResetBeforeSeed atau jalankan --seed-reset terlebih dahulu.");
            return;
        }

        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(department => department.Code)
            .ToListAsync(ct);

        var stageTypes = await context.StageTypes
            .AsNoTracking()
            .Where(stageType => stageType.IsActive)
            .ToListAsync(ct);

        var interviewerRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.Name == "Interviewer", ct);

        var hrEmployee = await context.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (departments.Count == 0
            || stageTypes.Count == 0
            || interviewerRole is null
            || hrEmployee is null)
        {
            logger.LogWarning(
                "[PerfSeed] Dilewati: master data (department/stage type/role/employee) belum lengkap. " +
                "Jalankan seeder master terlebih dahulu.");
            return;
        }

        var phase = Stopwatch.StartNew();

        var perfEmployees = await EnsureInterviewersAsync(
            context, passwordHasher, options, departments, interviewerRole, now, ct);
        LogPhase(logger, "Pewawancara", perfEmployees.Count, phase);

        phase.Restart();
        var batches = CreateBatches(options.Batches, departments, hrEmployee.Id, now);
        await InsertWithEfAsync(context, batches, options.BatchSize, ct);
        LogPhase(logger, "Batch rekrutmen", batches.Count, phase);

        phase.Restart();
        var orderedStageTypes = OrderStageTypes(stageTypes);
        var stageTypeById = orderedStageTypes.ToDictionary(stageType => stageType.Id);
        var batchStages = CreateBatchStages(batches, orderedStageTypes, now);
        await InsertWithEfAsync(context, batchStages, options.BatchSize, ct);
        LinkBatchStages(batchStages);
        await UpdateWithEfAsync(
            context, batchStages.Where(stage => stage.NextStageId.HasValue).ToList(), options.BatchSize, ct);
        LogPhase(logger, "Batch stage", batchStages.Count, phase);

        phase.Restart();
        var batchStatusById = batches.ToDictionary(batch => batch.Id, batch => batch.Status);
        var stageEmployees = CreateStageEmployees(
            batchStages, perfEmployees, stageTypeById, batchStatusById, now);
        await InsertWithEfAsync(context, stageEmployees, options.BatchSize, ct);
        LogPhase(logger, "Penugasan pewawancara", stageEmployees.Count, phase);

        phase.Restart();
        var candidates = PerformanceDataGenerator.CreateCandidates(options.Candidates, now);
        await InsertCandidatesAsync(context, candidates, options, ct);
        LogPhase(logger, "Kandidat", candidates.Count, phase);

        var stageById = batchStages.ToDictionary(stage => stage.Id);
        var stagesByBatch = batchStages
            .GroupBy(stage => stage.RecruitmentBatchId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<BatchStage>)group.OrderBy(stage => stage.StartDate).ToList());

        var testStageTypes = orderedStageTypes
            .Where(stageType => stageType.StageCategory == StageCategory.Test)
            .ToList();
        var questionsByStageTypeId = await LoadTestQuestionsAsync(context, testStageTypes, logger, ct);

        phase.Restart();
        var eligibleBatches = batches
            .Where(batch => batch.Status != BatchStatus.Draft)
            .ToList();
        var applicationsPerCandidate = Math.Min(
            options.ApplicationsPerCandidate,
            Math.Max(1, eligibleBatches.Count));
        var applicationResult = CreateApplicationsAndStages(
            candidates,
            eligibleBatches,
            stagesByBatch,
            stageTypeById,
            applicationsPerCandidate,
            random,
            now);

        // Jawaban dibuat lebih dulu karena TotalScore stage tes dihitung dari snapshot jawaban.
        var answers = CreateCandidateAnswers(
            applicationResult.ApplicationStages,
            stageById,
            stageTypeById,
            questionsByStageTypeId,
            options.CandidateAnswerRatio,
            random);

        await InsertApplicationsAsync(context, applicationResult.Applications, options, ct);
        await InsertApplicationStagesAsync(context, applicationResult.ApplicationStages, options, ct);
        await InsertCandidateAnswersAsync(context, answers, options, ct);
        LogPhase(logger,
            "Lamaran, tahapan & jawaban",
            applicationResult.Applications.Count + applicationResult.ApplicationStages.Count + answers.Count,
            phase);

        phase.Restart();
        var stageEmployeesByStageId = stageEmployees
            .GroupBy(stageEmployee => stageEmployee.BatchStageId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<StageEmployee>)group.ToList());
        var applicationById = applicationResult.Applications.ToDictionary(application => application.Id);
        var candidateById = candidates.ToDictionary(candidate => candidate.Id);
        var applicationStageById = applicationResult.ApplicationStages.ToDictionary(stage => stage.Id);

        var sessionResult = CreateInterviewSessions(
            applicationResult.ApplicationStages,
            stageById,
            stageTypeById,
            stageEmployeesByStageId,
            options.InterviewSessionRatio,
            hrEmployee.Id,
            random,
            now);

        await InsertWithEfAsync(context, sessionResult.Sessions, options.BatchSize, ct);
        await InsertWithEfAsync(context, sessionResult.SessionInterviews, options.BatchSize, ct);
        await InsertWithEfAsync(context, sessionResult.Assessments, options.BatchSize, ct);
        LogPhase(logger,
            "Sesi wawancara & penilaian",
            sessionResult.Sessions.Count + sessionResult.SessionInterviews.Count + sessionResult.Assessments.Count,
            phase);

        phase.Restart();
        var rescheduleRequests = CreateRescheduleRequests(
            sessionResult.Sessions,
            applicationStageById,
            applicationById,
            candidateById,
            perfEmployees,
            options.RescheduleRequests,
            hrEmployee.Id,
            random,
            now);
        await InsertWithEfAsync(context, rescheduleRequests, options.BatchSize, ct);
        LogPhase(logger, "Permintaan reschedule", rescheduleRequests.Count, phase);

        phase.Restart();
        var emailLogs = PerformanceDataGenerator.CreateEmailLogs(
            options.EmailLogs, candidates, perfEmployees, now, random);
        await InsertEmailLogsAsync(context, emailLogs, options, ct);
        LogPhase(logger, "Log email", emailLogs.Count, phase);

        context.ChangeTracker.Clear();

        logger.LogInformation(
            "[PerfSeed] Selesai dalam {Elapsed:n1} dtk. Kandidat={Candidates}, Lamaran={Applications}, " +
            "Tahapan={Stages}, Jawaban={Answers}, Sesi={Sessions}, EmailLog={Emails}.",
            totalStopwatch.Elapsed.TotalSeconds,
            candidates.Count,
            applicationResult.Applications.Count,
            applicationResult.ApplicationStages.Count,
            answers.Count,
            sessionResult.Sessions.Count,
            emailLogs.Count);
    }

    /// <summary>
    /// Menghapus seluruh data transaksional yang ditandai sebagai data perf.
    /// Data master (department, stage type, employee perf) dibiarkan agar dapat dipakai ulang.
    /// </summary>
    public static async Task ClearAsync(RmsDbContext context, CancellationToken ct = default)
    {
        var previousTimeout = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(Math.Max(previousTimeout ?? 0, 180));

        try
        {
            var hasPerformanceData = await context.RecruitmentBatches
                    .AnyAsync(batch => batch.BatchCode.StartsWith(BatchPrefix), ct)
                || await context.Candidates
                    .AnyAsync(candidate => candidate.Email.StartsWith(CandidateEmailPrefix)
                        && candidate.Email.EndsWith(CandidateEmailDomain), ct)
                || await context.EmailLogs
                    .AnyAsync(log => log.RecipientEmail.StartsWith(CandidateEmailPrefix)
                        || log.RecipientEmail.StartsWith(EmployeeEmailPrefix)
                        || log.Subject.StartsWith("PERF"), ct);

            if (!hasPerformanceData)
                return;

            var perfBatchIds = context.RecruitmentBatches
                .Where(batch => batch.BatchCode.StartsWith(BatchPrefix))
                .Select(batch => batch.Id);

            var perfCandidateIds = context.Candidates
                .Where(candidate => candidate.Email.StartsWith(CandidateEmailPrefix)
                    && candidate.Email.EndsWith(CandidateEmailDomain))
                .Select(candidate => candidate.Id);

            var perfApplicationIds = context.BatchApplications
                .Where(application => perfBatchIds.Contains(application.RecruitmentBatchId))
                .Select(application => application.Id);

            var perfApplicationStageIds = context.ApplicationStages
                .Where(stage => perfApplicationIds.Contains(stage.BatchApplicationId))
                .Select(stage => stage.Id);

            var perfBatchStageIds = context.BatchStages
                .Where(stage => perfBatchIds.Contains(stage.RecruitmentBatchId))
                .Select(stage => stage.Id);

            var perfSessionIds = context.InterviewSessions
                .Where(session => perfApplicationStageIds.Contains(session.ApplicationStageId))
                .Select(session => session.Id);

            var perfSessionInterviewIds = context.SessionInterviews
                .Where(sessionInterview => perfSessionIds.Contains(sessionInterview.InterviewSessionId))
                .Select(sessionInterview => sessionInterview.Id);

            await context.PublicApplicationVerifications
                .Where(verification => verification.Email.StartsWith(CandidateEmailPrefix)
                    || (verification.BatchId.HasValue && perfBatchIds.Contains(verification.BatchId.Value)))
                .ExecuteDeleteAsync(ct);

            await context.CandidateAnswers
                .Where(answer => perfApplicationStageIds.Contains(answer.ApplicationStageId))
                .ExecuteDeleteAsync(ct);

            await context.InterviewAssessments
                .Where(assessment => perfSessionInterviewIds.Contains(assessment.SessionInterviewId))
                .ExecuteDeleteAsync(ct);

            await context.RescheduleRequests
                .Where(request => perfSessionIds.Contains(request.InterviewSessionId))
                .ExecuteDeleteAsync(ct);

            await context.SessionInterviews
                .Where(sessionInterview => perfSessionIds.Contains(sessionInterview.InterviewSessionId))
                .ExecuteDeleteAsync(ct);

            await context.InterviewSessions
                .Where(session => perfApplicationStageIds.Contains(session.ApplicationStageId))
                .ExecuteDeleteAsync(ct);

            await context.StageEmployees
                .Where(stageEmployee => perfBatchStageIds.Contains(stageEmployee.BatchStageId))
                .ExecuteDeleteAsync(ct);

            await context.ApplicationStages
                .Where(stage => perfApplicationIds.Contains(stage.BatchApplicationId))
                .ExecuteDeleteAsync(ct);

            await context.BatchApplications
                .Where(application => perfBatchIds.Contains(application.RecruitmentBatchId))
                .ExecuteDeleteAsync(ct);

            await context.BatchStages
                .Where(stage => perfBatchIds.Contains(stage.RecruitmentBatchId))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(stage => stage.NextStageId, (Guid?)null),
                    ct);

            await context.BatchStages
                .Where(stage => perfBatchIds.Contains(stage.RecruitmentBatchId))
                .ExecuteDeleteAsync(ct);

            await context.RecruitmentBatches
                .Where(batch => perfBatchIds.Contains(batch.Id))
                .ExecuteDeleteAsync(ct);

            await context.EmailLogs
                .Where(log => log.RecipientEmail.StartsWith(CandidateEmailPrefix)
                    || log.RecipientEmail.StartsWith(EmployeeEmailPrefix)
                    || log.Subject.StartsWith("PERF"))
                .ExecuteDeleteAsync(ct);

            await context.Candidates
                .Where(candidate => perfCandidateIds.Contains(candidate.Id)
                    && !context.BatchApplications.Any(
                        application => application.CandidateId == candidate.Id))
                .ExecuteDeleteAsync(ct);
        }
        finally
        {
            context.Database.SetCommandTimeout(previousTimeout);
        }
    }

    private static async Task<List<Employee>> EnsureInterviewersAsync(
        RmsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        PerformanceSeedOptions options,
        IReadOnlyList<Department> departments,
        Role interviewerRole,
        DateTime now,
        CancellationToken ct)
    {
        var existingEmployees = await context.Employees
            .Where(employee => employee.Email.StartsWith(EmployeeEmailPrefix)
                && employee.Email.EndsWith(EmployeeEmailDomain))
            .OrderBy(employee => employee.EmployeeNumber)
            .ToListAsync(ct);

        var result = existingEmployees.ToList();
        var existingEmails = existingEmployees
            .Select(employee => employee.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newEmployees = new List<Employee>();
        var index = 0;

        while (result.Count + newEmployees.Count < options.Interviewers)
        {
            var email = PerformanceDataGenerator.PerfEmployeeEmail(index);
            if (existingEmails.Add(email))
            {
                var department = departments[index % departments.Count];
                newEmployees.Add(PerformanceDataGenerator.CreatePerfEmployee(index, department.Id, now));
            }

            index++;

            if (index > 100_000)
                break;
        }

        if (newEmployees.Count > 0)
        {
            await InsertWithEfAsync(context, newEmployees, options.BatchSize, ct);
            result.AddRange(newEmployees);
        }

        var existingUsernames = await context.Accounts
            .Where(account => account.Username.StartsWith("perf.emp."))
            .Select(account => account.Username)
            .ToListAsync(ct);

        var usernameSet = existingUsernames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sharedPasswordHash = passwordHasher.HashPassword(
            new Account { Id = Guid.Empty }, DefaultPassword);
        var newAccounts = new List<Account>();

        foreach (var employee in result)
        {
            var prefix = employee.Email.Split('@')[0];
            var suffix = prefix.Replace("perf.emp.", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (!int.TryParse(suffix, out var number) || number <= 0)
                continue;

            var username = $"perf.emp.{number:D3}";
            if (!usernameSet.Add(username))
                continue;

            newAccounts.Add(PerformanceDataGenerator.CreatePerfAccount(
                employee.Id, interviewerRole.Id, number - 1, sharedPasswordHash, now));
        }

        if (newAccounts.Count > 0)
            await InsertWithEfAsync(context, newAccounts, options.BatchSize, ct);

        return result;
    }

    private static List<RecruitmentBatch> CreateBatches(
        int batchCount,
        IReadOnlyList<Department> departments,
        Guid createdBy,
        DateTime now)
    {
        var openCount = Math.Clamp((int)Math.Round(batchCount * 0.5), 1, batchCount);
        var closedCount = Math.Clamp((int)Math.Round(batchCount * 0.35), 0, batchCount - openCount);
        var batches = new List<RecruitmentBatch>(batchCount);

        for (var index = 0; index < batchCount; index++)
        {
            BatchStatus status;
            DateTime startDate;
            DateTime endDate;
            DateTime deadline;

            if (index < openCount)
            {
                status = BatchStatus.Open;
                startDate = now.AddDays(-140 + index % 20).Date;
                endDate = now.AddDays(45 + index % 30).Date.AddHours(23).AddMinutes(59);
                deadline = startDate.AddDays(10 + index % 10).Date.AddHours(23).AddMinutes(59);
            }
            else if (index < openCount + closedCount)
            {
                status = BatchStatus.Closed;
                endDate = now.AddDays(-40 - index % 10 * 7).Date.AddHours(23).AddMinutes(59);
                startDate = endDate.AddDays(-90 - index % 5 * 10);
                deadline = endDate.AddDays(-30);
            }
            else
            {
                status = BatchStatus.Draft;
                startDate = now.AddDays(30 + index % 10 * 3).Date;
                endDate = startDate.AddDays(90);
                deadline = startDate.AddDays(60);
            }

            batches.Add(new RecruitmentBatch
            {
                Id = Guid.CreateVersion7(),
                DepartmentId = departments[index % departments.Count].Id,
                BatchCode = $"{BatchPrefix}{now:yyyy}-{index + 1:D3}",
                Name = $"Perf Recruitment {index + 1:D3}",
                Description = "Data simulasi performa untuk pengujian endpoint batch, aplikasi, dan dashboard.",
                StartDate = startDate,
                EndDate = endDate,
                ApplicationDeadlineAt = deadline,
                Status = status,
                CreatedBy = createdBy,
                CreatedAt = status == BatchStatus.Draft
                    ? now.AddDays(-(batchCount - index) * 3)
                    : startDate.AddDays(-(5 + index % 5))
            });
        }

        return batches;
    }

    private static List<StageType> OrderStageTypes(IReadOnlyList<StageType> stageTypes)
    {
        var preferredOrder = new[]
        {
            "Psikotes", "Logical Test", "Technical Test", "Interview HR", "Interview User"
        };

        var ordered = new List<StageType>(stageTypes.Count);
        foreach (var name in preferredOrder)
        {
            var match = stageTypes.FirstOrDefault(stageType =>
                string.Equals(stageType.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                ordered.Add(match);
        }

        ordered.AddRange(stageTypes.Where(stageType =>
            ordered.All(existing => existing.Id != stageType.Id)));

        return ordered;
    }

    private static List<BatchStage> CreateBatchStages(
        IReadOnlyList<RecruitmentBatch> batches,
        IReadOnlyList<StageType> orderedStageTypes,
        DateTime now)
    {
        var stages = new List<BatchStage>();

        for (var batchIndex = 0; batchIndex < batches.Count; batchIndex++)
        {
            var batch = batches[batchIndex];
            var stageCount = Math.Min(4 + batchIndex % 2, orderedStageTypes.Count);
            if (stageCount <= 0)
                continue;

            var windowStart = batch.ApplicationDeadlineAt ?? batch.StartDate;
            if (windowStart < batch.StartDate || windowStart >= batch.EndDate)
                windowStart = batch.StartDate;

            var totalDays = Math.Max(stageCount, (batch.EndDate - windowStart).TotalDays);
            var segmentDays = totalDays / stageCount;
            var activeIndex = -1;

            if (batch.Status == BatchStatus.Open)
            {
                for (var index = 0; index < stageCount; index++)
                {
                    var stageEnd = index == stageCount - 1
                        ? batch.EndDate
                        : windowStart.AddDays(segmentDays * (index + 1));

                    if (now < stageEnd)
                    {
                        activeIndex = index;
                        break;
                    }
                }

                if (activeIndex < 0)
                    activeIndex = stageCount - 1;
            }

            for (var stageIndex = 0; stageIndex < stageCount; stageIndex++)
            {
                var stageType = orderedStageTypes[stageIndex];
                var startDate = windowStart.AddDays(segmentDays * stageIndex);
                var endDate = stageIndex == stageCount - 1
                    ? batch.EndDate
                    : windowStart.AddDays(segmentDays * (stageIndex + 1));

                if (endDate <= startDate)
                    endDate = startDate.AddDays(1);

                var lifecycleStatus = batch.Status switch
                {
                    BatchStatus.Closed => BatchStageLifecycleStatus.Completed,
                    BatchStatus.Draft => BatchStageLifecycleStatus.Inactive,
                    _ when stageIndex < activeIndex => BatchStageLifecycleStatus.Completed,
                    _ when stageIndex == activeIndex => BatchStageLifecycleStatus.Active,
                    _ => BatchStageLifecycleStatus.Inactive
                };

                var durationMinutes = stageType.StageCategory == StageCategory.Test
                    ? string.Equals(stageType.Name, "Psikotes", StringComparison.OrdinalIgnoreCase) ? 15 : 30
                    : 60;

                stages.Add(new BatchStage
                {
                    Id = Guid.CreateVersion7(),
                    NextStageId = null,
                    RecruitmentBatchId = batch.Id,
                    StageTypeId = stageType.Id,
                    StageName = stageType.Name,
                    LifecycleStatus = lifecycleStatus,
                    PassingScore = stageType.StageCategory == StageCategory.Test
                        ? 65m + batchIndex % 4 * 5m
                        : null,
                    StartDate = startDate,
                    EndDate = endDate,
                    PlannedDurationTicks = TimeSpan.FromMinutes(durationMinutes).Ticks,
                    CreatedAt = batch.CreatedAt
                });
            }
        }

        return stages;
    }

    private static void LinkBatchStages(IReadOnlyList<BatchStage> stages)
    {
        foreach (var group in stages.GroupBy(stage => stage.RecruitmentBatchId))
        {
            var ordered = group.OrderBy(stage => stage.StartDate).ToList();
            for (var index = 0; index < ordered.Count - 1; index++)
                ordered[index].NextStageId = ordered[index + 1].Id;
        }
    }

    private static List<StageEmployee> CreateStageEmployees(
        IReadOnlyList<BatchStage> stages,
        IReadOnlyList<Employee> employees,
        IReadOnlyDictionary<Guid, StageType> stageTypeById,
        IReadOnlyDictionary<Guid, BatchStatus> batchStatusById,
        DateTime now)
    {
        var result = new List<StageEmployee>();
        if (employees.Count == 0)
            return result;

        var employeeCursor = 0;

        for (var stageIndex = 0; stageIndex < stages.Count; stageIndex++)
        {
            var stage = stages[stageIndex];
            if (!stageTypeById.TryGetValue(stage.StageTypeId, out var stageType)
                || stageType.StageCategory != StageCategory.Interview)
            {
                continue;
            }

            if (batchStatusById.TryGetValue(stage.RecruitmentBatchId, out var batchStatus)
                && batchStatus == BatchStatus.Draft)
            {
                continue;
            }

            // Round-robin agar seluruh akun pewawancara memperoleh penugasan,
            // termasuk pada batch yang sudah selesai dan memiliki sesi wawancara.
            var assignmentCount = Math.Min(4, employees.Count);
            var usedEmployeeIds = new HashSet<Guid>();

            for (var position = 0; position < assignmentCount; position++)
            {
                var employee = employees[(employeeCursor + position) % employees.Count];
                if (!usedEmployeeIds.Add(employee.Id))
                    continue;

                // Sebagian besar panel harus Accepted agar sesi wawancara benar-benar
                // terbentuk; variasi Pending/Declined dipakai untuk menguji alur undangan.
                var status = position switch
                {
                    3 when stageIndex % 3 == 0 => StageEmployeeStatus.Declined,
                    3 when stageIndex % 3 == 1 => StageEmployeeStatus.Pending,
                    _ => StageEmployeeStatus.Accepted
                };

                result.Add(new StageEmployee
                {
                    Id = Guid.CreateVersion7(),
                    BatchStageId = stage.Id,
                    EmployeeId = employee.Id,
                    Status = status,
                    IsActive = status != StageEmployeeStatus.Declined,
                    Note = "Perf assignment",
                    CreatedAt = now.AddDays(-(stageIndex % 30))
                });
            }

            employeeCursor += assignmentCount;
        }

        return result;
    }

    private static ApplicationSeedResult CreateApplicationsAndStages(
        IReadOnlyList<Candidate> candidates,
        IReadOnlyList<RecruitmentBatch> eligibleBatches,
        IReadOnlyDictionary<Guid, IReadOnlyList<BatchStage>> stagesByBatch,
        IReadOnlyDictionary<Guid, StageType> stageTypeById,
        int applicationsPerCandidate,
        Random random,
        DateTime now)
    {
        var applications = new List<BatchApplication>();
        var applicationStages = new List<ApplicationStage>();

        if (eligibleBatches.Count == 0)
            return new ApplicationSeedResult(applications, applicationStages);

        var pairs = PerformanceDataGenerator.CreateApplicationPairs(
            candidates.Count, eligibleBatches.Count, applicationsPerCandidate);

        foreach (var (candidateIndex, batchIndex) in pairs)
        {
            var candidate = candidates[candidateIndex];
            var batch = eligibleBatches[batchIndex];

            if (!stagesByBatch.TryGetValue(batch.Id, out var batchStages) || batchStages.Count == 0)
                continue;

            var codeApplication = $"PERF-{batchIndex:D2}-{candidateIndex:D6}";
            var status = ChooseApplicationStatus(batch.Status, random);

            // Jangan biarkan lamaran berada di tahap yang belum dimulai pada batch Open.
            var startedStageCount = batchStages.Count(batchStage => batchStage.StartDate <= now);
            var maxStageCount = Math.Max(1, startedStageCount);
            if (status == ApplicationStatus.Accepted && maxStageCount < batchStages.Count)
                status = ApplicationStatus.InProcess;

            var appliedAt = ChooseAppliedAt(batch, now, random);

            var finalResultStatus = status switch
            {
                ApplicationStatus.Accepted => FinalResultStatus.Accepted,
                ApplicationStatus.Rejected => FinalResultStatus.Rejected,
                _ => (FinalResultStatus?)null
            };

            var offeringStatus = status == ApplicationStatus.Accepted
                ? ChooseOfferingStatus(random)
                : OfferingLetterStatus.Pending;

            var application = new BatchApplication
            {
                Id = Guid.CreateVersion7(),
                CodeApplication = codeApplication,
                AppliedAt = appliedAt,
                FinalResultStatus = finalResultStatus,
                Status = status,
                Notes = "Data uji performa",
                OfferingLetterStatus = offeringStatus,
                FileOfferingLetter = status == ApplicationStatus.Accepted
                    && offeringStatus != OfferingLetterStatus.Pending
                        ? $"perf/offering/{codeApplication}.pdf"
                        : string.Empty,
                CandidateId = candidate.Id,
                RecruitmentBatchId = batch.Id,
                ExpectedSalary = candidate.ExpectedSalary + random.Next(0, 4) * 250_000m,
                IsStandalone = false,
                CreatedAt = appliedAt.AddMinutes(random.Next(0, 120))
            };

            applications.Add(application);

            var reachedStageCount = status switch
            {
                ApplicationStatus.Accepted => batchStages.Count,
                _ => Math.Min(maxStageCount, 1 + random.Next(batchStages.Count))
            };

            for (var stageIndex = 0; stageIndex < reachedStageCount; stageIndex++)
            {
                var batchStage = batchStages[stageIndex];
                var stageType = stageTypeById[batchStage.StageTypeId];
                var isTest = stageType.StageCategory == StageCategory.Test;
                var isLastStage = stageIndex == reachedStageCount - 1;

                var stageStatus = ChooseApplicationStageStatus(
                    status, isLastStage, random);

                var durationMinutes = Math.Max(
                    5,
                    (int)TimeSpan.FromTicks(
                        batchStage.PlannedDurationTicks
                        ?? TimeSpan.FromMinutes(30).Ticks).TotalMinutes);

                DateTime? attemptStartedAt = null;
                DateTime? completedAt = null;
                TimeSpan? durationTest = null;
                var isAttempts = false;

                if (isTest && stageStatus != ApplicationStageStatus.InProgress)
                {
                    attemptStartedAt = batchStage.StartDate.AddMinutes(2);
                    durationTest = TimeSpan.FromMinutes(durationMinutes);
                    completedAt = attemptStartedAt.Value.AddMinutes(random.Next(5, durationMinutes + 1));
                    isAttempts = true;
                }
                else if (isTest)
                {
                    attemptStartedAt = now.AddMinutes(-random.Next(1, durationMinutes));
                    durationTest = TimeSpan.FromMinutes(durationMinutes);
                    isAttempts = true;
                }
                else if (stageStatus is ApplicationStageStatus.Prioritized
                         or ApplicationStageStatus.Considered
                         or ApplicationStageStatus.Rejected
                         or ApplicationStageStatus.Dropped)
                {
                    completedAt = batchStage.EndDate < now ? batchStage.EndDate : now;
                }

                applicationStages.Add(new ApplicationStage
                {
                    Id = Guid.CreateVersion7(),
                    Status = stageStatus,
                    TotalScore = null,
                    StartedAt = batchStage.StartDate,
                    EndAt = batchStage.EndDate,
                    AttemptStartedAt = attemptStartedAt,
                    CompletedAt = completedAt,
                    Notes = stageStatus switch
                    {
                        ApplicationStageStatus.Rejected => "Tidak lolos tahap",
                        ApplicationStageStatus.Dropped => "Kandidat gugur",
                        ApplicationStageStatus.Prioritized or ApplicationStageStatus.Considered
                            => "Lolos tahap",
                        _ => null
                    },
                    IsAttempts = isAttempts,
                    DurationTest = durationTest,
                    BatchApplicationId = application.Id,
                    BatchStageId = batchStage.Id,
                    CreatedAt = batchStage.StartDate.AddMinutes(-15)
                });
            }
        }

        return new ApplicationSeedResult(applications, applicationStages);
    }

    private static ApplicationStatus ChooseApplicationStatus(BatchStatus batchStatus, Random random)
    {
        var roll = random.Next(100);
        var status = roll < 55
            ? ApplicationStatus.InProcess
            : roll < 75
                ? ApplicationStatus.Rejected
                : roll < 85
                    ? ApplicationStatus.Dropped
                    : ApplicationStatus.Accepted;

        if (batchStatus == BatchStatus.Closed && status == ApplicationStatus.InProcess)
        {
            status = random.Next(3) switch
            {
                0 => ApplicationStatus.Accepted,
                1 => ApplicationStatus.Rejected,
                _ => ApplicationStatus.Dropped
            };
        }

        return status;
    }

    private static ApplicationStageStatus ChooseApplicationStageStatus(
        ApplicationStatus applicationStatus,
        bool isLastStage,
        Random random)
    {
        if (!isLastStage)
        {
            return random.Next(2) == 0
                ? ApplicationStageStatus.Prioritized
                : ApplicationStageStatus.Considered;
        }

        return applicationStatus switch
        {
            ApplicationStatus.Accepted => random.Next(2) == 0
                ? ApplicationStageStatus.Prioritized
                : ApplicationStageStatus.Considered,
            ApplicationStatus.Rejected => ApplicationStageStatus.Rejected,
            ApplicationStatus.Dropped => ApplicationStageStatus.Dropped,
            _ => ApplicationStageStatus.InProgress
        };
    }

    private static OfferingLetterStatus ChooseOfferingStatus(Random random)
    {
        var roll = random.Next(100);
        return roll < 50
            ? OfferingLetterStatus.Approved
            : roll < 70
                ? OfferingLetterStatus.Negotiate
                : roll < 80
                    ? OfferingLetterStatus.Rejected
                    : OfferingLetterStatus.Pending;
    }

    private static DateTime ChooseAppliedAt(RecruitmentBatch batch, DateTime now, Random random)
    {
        var earliest = batch.StartDate;
        var latest = batch.ApplicationDeadlineAt ?? batch.EndDate;
        if (latest > now)
            latest = now;
        if (latest <= earliest)
            latest = earliest.AddDays(1);

        return earliest.AddDays((latest - earliest).TotalDays * random.NextDouble());
    }

    private static async Task<Dictionary<Guid, List<TestQuestionTemplate>>> LoadTestQuestionsAsync(
        RmsDbContext context,
        IReadOnlyList<StageType> stageTypes,
        ILogger logger,
        CancellationToken ct)
    {
        var result = new Dictionary<Guid, List<TestQuestionTemplate>>();
        if (stageTypes.Count == 0)
            return result;

        var stageTypeIds = stageTypes.Select(stageType => stageType.Id).ToList();
        var banks = await context.QuestionBanks
            .AsNoTracking()
            .Include(questionBank => questionBank.Questions)
                .ThenInclude(question => question.QuestionOptions)
            .Where(questionBank =>
                stageTypeIds.Contains(questionBank.StageTypeId)
                && questionBank.Questions.Any())
            .ToListAsync(ct);

        foreach (var stageType in stageTypes)
        {
            var bank = banks
                .FirstOrDefault(candidate => candidate.StageTypeId == stageType.Id);

            if (bank is null)
            {
                logger.LogWarning(
                    "[PerfSeed] Bank soal untuk stage type {StageType} tidak ditemukan; jawaban kandidat dilewati.",
                    stageType.Name);
                result[stageType.Id] = [];
                continue;
            }

            result[stageType.Id] = bank.Questions
                .Where(question => question.IsActive)
                .OrderBy(question => question.SequenceNumber)
                .Select(question => new TestQuestionTemplate(
                    question.QuestionText,
                    question.DiscType,
                    question.QuestionOptions
                        .Where(option => option.IsActive)
                        .OrderBy(option => option.SequenceNumber)
                        .ToList()))
                .Where(template => template.Options.Count > 0)
                .ToList();
        }

        return result;
    }

    private static List<CandidateAnswer> CreateCandidateAnswers(
        IReadOnlyList<ApplicationStage> applicationStages,
        IReadOnlyDictionary<Guid, BatchStage> stageById,
        IReadOnlyDictionary<Guid, StageType> stageTypeById,
        IReadOnlyDictionary<Guid, List<TestQuestionTemplate>> questionsByStageTypeId,
        double answerRatio,
        Random random)
    {
        var answers = new List<CandidateAnswer>();

        foreach (var stage in applicationStages)
        {
            if (!stageById.TryGetValue(stage.BatchStageId, out var batchStage)
                || !stageTypeById.TryGetValue(batchStage.StageTypeId, out var stageType)
                || stageType.StageCategory != StageCategory.Test)
            {
                continue;
            }

            if (stage.CompletedAt is null)
                continue;

            if (!questionsByStageTypeId.TryGetValue(batchStage.StageTypeId, out var questions)
                || questions.Count == 0)
            {
                continue;
            }

            if (random.NextDouble() >= answerRatio)
                continue;

            var maximumScore = questions.Sum(question => question.Options.Max(option => option.Score));
            if (maximumScore <= 0)
                continue;

            var stageAnswers = new List<(CandidateAnswer Answer, TestQuestionTemplate Question)>(questions.Count);

            for (var index = 0; index < questions.Count; index++)
            {
                var question = questions[index];
                var option = question.Options[random.Next(question.Options.Count)];
                var answer = new CandidateAnswer
                {
                    Id = Guid.CreateVersion7(),
                    ApplicationStageId = stage.Id,
                    DiscType = question.DiscType,
                    QuestionText = question.QuestionText,
                    QuestionOptionText = option.OptionText,
                    Score = option.Score,
                    CreatedAt = stage.CompletedAt.Value.AddMinutes(index)
                };

                answers.Add(answer);
                stageAnswers.Add((answer, question));
            }

            AdjustTestAnswers(stageAnswers, maximumScore, stage.Status, batchStage.PassingScore);
            stage.TotalScore = CalculateTestNormalizedScore(stageAnswers, maximumScore);
        }

        return answers;
    }

    private static void AdjustTestAnswers(
        IReadOnlyList<(CandidateAnswer Answer, TestQuestionTemplate Question)> stageAnswers,
        decimal maximumScore,
        ApplicationStageStatus stageStatus,
        decimal? passingScore)
    {
        if (passingScore is null || stageAnswers.Count == 0)
            return;

        var isRejected = stageStatus == ApplicationStageStatus.Rejected;
        var shouldAdjust = isRejected
            || stageStatus is ApplicationStageStatus.Submitted
                or ApplicationStageStatus.Considered
                or ApplicationStageStatus.Prioritized;
        if (!shouldAdjust)
            return;

        var target = isRejected
            ? Math.Max(0m, passingScore.Value - 1m)
            : Math.Min(100m, passingScore.Value + 1m);

        var maxAttempts = stageAnswers.Count * 3;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var current = CalculateTestNormalizedScore(stageAnswers, maximumScore);
            if (isRejected ? current < target : current >= target)
                break;

            var item = stageAnswers[attempt % stageAnswers.Count];
            var targetOption = isRejected
                ? item.Question.Options.OrderBy(option => option.Score).First()
                : item.Question.Options.OrderByDescending(option => option.Score).First();

            if (item.Answer.Score == targetOption.Score)
                continue;

            item.Answer.QuestionOptionText = targetOption.OptionText;
            item.Answer.Score = targetOption.Score;
        }
    }

    private static decimal CalculateTestNormalizedScore(
        IReadOnlyList<(CandidateAnswer Answer, TestQuestionTemplate Question)> stageAnswers,
        decimal maximumScore)
        => maximumScore <= 0 || stageAnswers.Count == 0
            ? 0m
            : Math.Round(stageAnswers.Sum(item => item.Answer.Score) / maximumScore * 100m, 2);

    private static SessionSeedResult CreateInterviewSessions(
        IReadOnlyList<ApplicationStage> applicationStages,
        IReadOnlyDictionary<Guid, BatchStage> stageById,
        IReadOnlyDictionary<Guid, StageType> stageTypeById,
        IReadOnlyDictionary<Guid, IReadOnlyList<StageEmployee>> stageEmployeesByStageId,
        double sessionRatio,
        Guid createdBy,
        Random random,
        DateTime now)
    {
        var sessions = new List<InterviewSession>();
        var sessionPlans = new List<SessionPlan>();

        foreach (var applicationStage in applicationStages)
        {
            if (!stageById.TryGetValue(applicationStage.BatchStageId, out var batchStage)
                || !stageTypeById.TryGetValue(batchStage.StageTypeId, out var stageType)
                || stageType.StageCategory != StageCategory.Interview)
            {
                continue;
            }

            if (random.NextDouble() >= sessionRatio)
                continue;

            if (!stageEmployeesByStageId.TryGetValue(batchStage.Id, out var assignments))
                continue;

            var acceptedInterviewers = assignments
                .Where(assignment => assignment.Status == StageEmployeeStatus.Accepted
                    && assignment.IsActive)
                .ToList();

            if (acceptedInterviewers.Count == 0)
                continue;

            var desiredPanelSize = Math.Min(1 + random.Next(3), acceptedInterviewers.Count);
            var startOffset = random.Next(acceptedInterviewers.Count);
            var panel = new List<StageEmployee>(desiredPanelSize);
            var usedEmployeeIds = new HashSet<Guid>();

            for (var offset = 0; offset < acceptedInterviewers.Count
                 && panel.Count < desiredPanelSize; offset++)
            {
                var stageEmployee = acceptedInterviewers[(startOffset + offset) % acceptedInterviewers.Count];
                if (usedEmployeeIds.Add(stageEmployee.EmployeeId))
                    panel.Add(stageEmployee);
            }

            if (panel.Count == 0)
                continue;

            var isTerminalStage = applicationStage.Status is ApplicationStageStatus.Considered
                or ApplicationStageStatus.Prioritized
                or ApplicationStageStatus.Rejected
                or ApplicationStageStatus.Dropped;

            DateTime scheduledStart;
            if (isTerminalStage)
            {
                scheduledStart = (applicationStage.CompletedAt ?? applicationStage.EndAt)
                    .AddDays(-random.Next(0, 4));
            }
            else if (random.Next(2) == 0)
            {
                scheduledStart = now.AddDays(-random.Next(1, 30));
            }
            else
            {
                scheduledStart = now.AddDays(random.Next(1, 21));
            }

            scheduledStart = DateTime.SpecifyKind(
                scheduledStart.Date.AddHours(8 + random.Next(0, 9))
                    .AddMinutes(random.Next(0, 4) * 15),
                DateTimeKind.Utc);
            var scheduledEnd = scheduledStart.AddMinutes(45 + random.Next(0, 3) * 15);
            var isPast = scheduledEnd < now;

            var sessionStatus = ChooseInterviewSessionStatus(isPast, isTerminalStage, random);
            var isOnline = random.Next(100) < 70;
            var sessionId = Guid.CreateVersion7();

            string? fileTakeHome = null;
            string? submissionFile = null;
            DateTime? endedSubmissionAt = null;

            if (isOnline && random.Next(100) < 25)
            {
                fileTakeHome = $"perf/takehome/{sessionId:N}.pdf";
                endedSubmissionAt = scheduledEnd.AddDays(3);

                if (random.Next(2) == 0)
                    submissionFile = $"perf/submission/{sessionId:N}.zip";
            }

            sessions.Add(new InterviewSession
            {
                Id = sessionId,
                SessionName = $"Wawancara {sessionId:N}",
                ScheduledStart = scheduledStart,
                ScheduledEnd = scheduledEnd,
                IsOnline = isOnline,
                Location = isOnline ? null : $"Ruang Meeting {random.Next(1, 10)}",
                MeetingUrl = isOnline ? $"https://meet.example.test/{sessionId:N}" : null,
                Status = sessionStatus,
                CreatedBy = createdBy,
                FileTakeHome = fileTakeHome,
                SubmissionFile = submissionFile,
                EndedSubmissionAt = endedSubmissionAt,
                ApplicationStageId = applicationStage.Id,
                CreatedAt = scheduledStart.AddDays(-3)
            });

            sessionPlans.Add(new SessionPlan(sessions[^1], panel, isPast));
        }

        var sessionInterviews = new List<SessionInterview>();
        var assessments = new List<InterviewAssessment>();

        foreach (var plan in sessionPlans)
        {
            for (var index = 0; index < plan.Panel.Count; index++)
            {
                var status = plan.IsPast
                    ? random.Next(100) < 90
                        ? SessionInterviewStatus.Completed
                        : SessionInterviewStatus.Cancelled
                    : SessionInterviewStatus.InProgress;

                sessionInterviews.Add(new SessionInterview
                {
                    Id = Guid.CreateVersion7(),
                    StageEmployeeId = plan.Panel[index].Id,
                    InterviewSessionId = plan.Session.Id,
                    Status = status,
                    IsLead = index == 0,
                    CreatedAt = plan.Session.CreatedAt
                });
            }
        }

        foreach (var sessionInterview in sessionInterviews)
        {
            if (sessionInterview.Status != SessionInterviewStatus.Completed
                || random.Next(100) >= 80)
            {
                continue;
            }

            assessments.Add(new InterviewAssessment
            {
                Id = Guid.CreateVersion7(),
                SessionInterviewId = sessionInterview.Id,
                Notes = "Catatan penilaian simulasi performa.",
                Recommendation = (Recommendation)(random.Next(4) + 1),
                Score = 60 + random.Next(41),
                CreatedAt = sessionInterview.CreatedAt.AddDays(1)
            });
        }

        return new SessionSeedResult(sessions, sessionInterviews, assessments);
    }

    private static InterviewSessionStatus ChooseInterviewSessionStatus(
        bool isPast,
        bool isTerminalStage,
        Random random)
    {
        if (isPast)
        {
            return random.Next(10) == 0
                ? InterviewSessionStatus.Rescheduled
                : InterviewSessionStatus.AcceptedByCandidate;
        }

        if (isTerminalStage)
            return InterviewSessionStatus.AcceptedByCandidate;

        var roll = random.Next(100);
        return roll < 35
            ? InterviewSessionStatus.PendingConfirmation
            : roll < 55
                ? InterviewSessionStatus.Scheduled
                : roll < 75
                    ? InterviewSessionStatus.AcceptedByInterviewer
                    : InterviewSessionStatus.AcceptedByCandidate;
    }

    private static List<RescheduleRequest> CreateRescheduleRequests(
        IReadOnlyList<InterviewSession> sessions,
        IReadOnlyDictionary<Guid, ApplicationStage> applicationStageById,
        IReadOnlyDictionary<Guid, BatchApplication> applicationById,
        IReadOnlyDictionary<Guid, Candidate> candidateById,
        IReadOnlyList<Employee> perfEmployees,
        int requestedCount,
        Guid reviewedBy,
        Random random,
        DateTime now)
    {
        var result = new List<RescheduleRequest>();
        if (requestedCount <= 0 || sessions.Count == 0)
            return result;

        var targetCount = Math.Min(requestedCount, sessions.Count);
        var targets = new List<InterviewSession>(targetCount);
        var usedIndexes = new HashSet<int>();

        while (targets.Count < targetCount)
        {
            var index = random.Next(sessions.Count);
            if (usedIndexes.Add(index))
                targets.Add(sessions[index]);
        }

        foreach (var session in targets)
        {
            if (!applicationStageById.TryGetValue(session.ApplicationStageId, out var applicationStage)
                || !applicationById.TryGetValue(applicationStage.BatchApplicationId, out var application)
                || !candidateById.TryGetValue(application.CandidateId, out var candidate))
            {
                continue;
            }

            var requesterEmail = random.Next(100) < 70
                ? candidate.Email
                : perfEmployees[random.Next(perfEmployees.Count)].Email;

            var roll = random.Next(100);
            var status = roll < 50
                ? RescheduleStatus.Pending
                : roll < 80
                    ? RescheduleStatus.Approved
                    : RescheduleStatus.Rejected;

            var proposedStart = session.ScheduledStart.AddDays(1 + random.Next(0, 7));

            result.Add(new RescheduleRequest
            {
                Id = Guid.CreateVersion7(),
                InterviewSessionId = session.Id,
                RequesterEmail = requesterEmail,
                ProposedStart = proposedStart,
                ProposedEnd = proposedStart.AddMinutes(45),
                Reason = RescheduleReasons[random.Next(RescheduleReasons.Length)],
                Status = status,
                ReviewedBy = status == RescheduleStatus.Pending ? null : reviewedBy,
                ReviewedAt = status == RescheduleStatus.Pending
                    ? null
                    : session.CreatedAt.AddHours(1 + random.Next(0, 48)),
                ReviewNotes = status == RescheduleStatus.Pending
                    ? null
                    : "Keputusan simulasi performa.",
                CreatedAt = session.CreatedAt.AddHours(1)
            });
        }

        return result;
    }

    private static async Task InsertCandidatesAsync(
        RmsDbContext context,
        IReadOnlyList<Candidate> candidates,
        PerformanceSeedOptions options,
        CancellationToken ct)
    {
        if (options.UseSqlBulkCopy)
        {
            await BulkInsertHelper.InsertAsync(
                context, "Candidates", candidates, options.BatchSize,
                CandidateColumns, FillCandidate, ct);
            return;
        }

        await InsertWithEfAsync(context, candidates, options.BatchSize, ct);
    }

    private static async Task InsertApplicationsAsync(
        RmsDbContext context,
        IReadOnlyList<BatchApplication> applications,
        PerformanceSeedOptions options,
        CancellationToken ct)
    {
        if (options.UseSqlBulkCopy)
        {
            await BulkInsertHelper.InsertAsync(
                context, "BatchApplications", applications, options.BatchSize,
                ApplicationColumns, FillApplication, ct);
            return;
        }

        await InsertWithEfAsync(context, applications, options.BatchSize, ct);
    }

    private static async Task InsertApplicationStagesAsync(
        RmsDbContext context,
        IReadOnlyList<ApplicationStage> applicationStages,
        PerformanceSeedOptions options,
        CancellationToken ct)
    {
        if (options.UseSqlBulkCopy)
        {
            await BulkInsertHelper.InsertAsync(
                context, "ApplicationStages", applicationStages, options.BatchSize,
                ApplicationStageColumns, FillApplicationStage, ct);
            return;
        }

        await InsertWithEfAsync(context, applicationStages, options.BatchSize, ct);
    }

    private static async Task InsertCandidateAnswersAsync(
        RmsDbContext context,
        IReadOnlyList<CandidateAnswer> answers,
        PerformanceSeedOptions options,
        CancellationToken ct)
    {
        if (options.UseSqlBulkCopy)
        {
            await BulkInsertHelper.InsertAsync(
                context, "CandidateAnswer", answers, options.BatchSize,
                CandidateAnswerColumns, FillCandidateAnswer, ct);
            return;
        }

        await InsertWithEfAsync(context, answers, options.BatchSize, ct);
    }

    private static async Task InsertEmailLogsAsync(
        RmsDbContext context,
        IReadOnlyList<EmailLog> emailLogs,
        PerformanceSeedOptions options,
        CancellationToken ct)
    {
        if (options.UseSqlBulkCopy)
        {
            await BulkInsertHelper.InsertAsync(
                context, "EmailLog", emailLogs, options.BatchSize,
                EmailLogColumns, FillEmailLog, ct);
            return;
        }

        await InsertWithEfAsync(context, emailLogs, options.BatchSize, ct);
    }

    private static async Task InsertWithEfAsync<T>(
        RmsDbContext context,
        IReadOnlyList<T> entities,
        int chunkSize,
        CancellationToken ct) where T : class
    {
        if (entities.Count == 0)
            return;

        var previousAutoDetect = context.ChangeTracker.AutoDetectChangesEnabled;
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            for (var offset = 0; offset < entities.Count; offset += chunkSize)
            {
                var count = Math.Min(chunkSize, entities.Count - offset);
                var chunk = new List<T>(count);

                for (var index = offset; index < offset + count; index++)
                    chunk.Add(entities[index]);

                context.AddRange(chunk);
                await context.SaveChangesAsync(ct);
                context.ChangeTracker.Clear();
            }
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
    }

    private static async Task UpdateWithEfAsync<T>(
        RmsDbContext context,
        IReadOnlyList<T> entities,
        int chunkSize,
        CancellationToken ct) where T : class
    {
        if (entities.Count == 0)
            return;

        var previousAutoDetect = context.ChangeTracker.AutoDetectChangesEnabled;
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            for (var offset = 0; offset < entities.Count; offset += chunkSize)
            {
                var count = Math.Min(chunkSize, entities.Count - offset);
                var chunk = new List<T>(count);

                for (var index = offset; index < offset + count; index++)
                    chunk.Add(entities[index]);

                context.UpdateRange(chunk);
                await context.SaveChangesAsync(ct);
                context.ChangeTracker.Clear();
            }
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
    }

    private static void FillCandidate(DataRow row, Candidate candidate)
    {
        row["Id"] = candidate.Id;
        row["IsInTalentPool"] = candidate.IsInTalentPool;
        row["FullName"] = candidate.FullName;
        row["Gender"] = (int)candidate.Gender;
        row["Email"] = candidate.Email;
        row["PhoneNumber"] = candidate.PhoneNumber;
        row["BirthDate"] = candidate.BirthDate.ToDateTime(TimeOnly.MinValue);
        row["Cv"] = (object?)candidate.Cv ?? DBNull.Value;
        row["Portfolio"] = (object?)candidate.Portfolio ?? DBNull.Value;
        row["Education"] = candidate.Education;
        row["Institution"] = candidate.Institution;
        row["Address"] = candidate.Address;
        row["ExpectedSalary"] = candidate.ExpectedSalary;
        row["Gpa"] = candidate.Gpa;
        row["CreatedAt"] = candidate.CreatedAt;
        row["UpdatedAt"] = (object?)candidate.UpdatedAt ?? DBNull.Value;
    }

    private static void FillApplication(DataRow row, BatchApplication application)
    {
        row["Id"] = application.Id;
        row["CodeApplication"] = application.CodeApplication;
        row["AppliedAt"] = application.AppliedAt;
        row["FinalResultStatus"] = application.FinalResultStatus.HasValue
            ? (int)application.FinalResultStatus.Value
            : DBNull.Value;
        row["Status"] = (int)application.Status;
        row["Notes"] = application.Notes;
        row["OfferingLetterStatus"] = (int)application.OfferingLetterStatus;
        row["FileOfferingLetter"] = application.FileOfferingLetter;
        row["CandidateId"] = application.CandidateId;
        row["RecruitmentBatchId"] = application.RecruitmentBatchId;
        row["ExpectedSalary"] = application.ExpectedSalary.HasValue
            ? application.ExpectedSalary.Value
            : DBNull.Value;
        row["IsStandalone"] = application.IsStandalone;
        row["CreatedAt"] = application.CreatedAt;
        row["UpdatedAt"] = (object?)application.UpdatedAt ?? DBNull.Value;
    }

    private static void FillApplicationStage(DataRow row, ApplicationStage applicationStage)
    {
        row["Id"] = applicationStage.Id;
        row["Status"] = (int)applicationStage.Status;
        row["TotalScore"] = applicationStage.TotalScore.HasValue
            ? applicationStage.TotalScore.Value
            : DBNull.Value;
        row["StartedAt"] = applicationStage.StartedAt;
        row["EndAt"] = applicationStage.EndAt;
        row["AttemptStartedAt"] = applicationStage.AttemptStartedAt.HasValue
            ? applicationStage.AttemptStartedAt.Value
            : DBNull.Value;
        row["CompletedAt"] = applicationStage.CompletedAt.HasValue
            ? applicationStage.CompletedAt.Value
            : DBNull.Value;
        row["Notes"] = (object?)applicationStage.Notes ?? DBNull.Value;
        row["IsAttempts"] = applicationStage.IsAttempts;
        row["DurationTest"] = applicationStage.DurationTest.HasValue
            ? applicationStage.DurationTest.Value
            : DBNull.Value;
        row["BatchApplicationId"] = applicationStage.BatchApplicationId;
        row["BatchStageId"] = applicationStage.BatchStageId;
        row["CreatedAt"] = applicationStage.CreatedAt;
        row["UpdatedAt"] = (object?)applicationStage.UpdatedAt ?? DBNull.Value;
    }

    private static void FillCandidateAnswer(DataRow row, CandidateAnswer answer)
    {
        row["Id"] = answer.Id;
        row["ApplicationStageId"] = answer.ApplicationStageId;
        row["DiscType"] = (int)answer.DiscType;
        row["QuestionText"] = answer.QuestionText;
        row["QuestionOptionText"] = answer.QuestionOptionText;
        row["Score"] = answer.Score;
        row["CreatedAt"] = answer.CreatedAt;
        row["UpdatedAt"] = (object?)answer.UpdatedAt ?? DBNull.Value;
    }

    private static void FillEmailLog(DataRow row, EmailLog emailLog)
    {
        row["Id"] = emailLog.Id;
        row["SenderEmail"] = emailLog.SenderEmail;
        row["RecipientEmail"] = emailLog.RecipientEmail;
        row["Type"] = (int)emailLog.Type;
        row["Subject"] = emailLog.Subject;
        row["Body"] = emailLog.Body;
        row["HtmlBody"] = (object?)emailLog.HtmlBody ?? DBNull.Value;
        row["Status"] = (int)emailLog.Status;
        row["AttemptedAt"] = emailLog.AttemptedAt;
        row["SentAt"] = emailLog.SentAt.HasValue ? emailLog.SentAt.Value : DBNull.Value;
        row["ErrorMessage"] = (object?)emailLog.ErrorMessage ?? DBNull.Value;
    }

    private static void LogPhase(ILogger logger, string phaseName, int count, Stopwatch stopwatch)
        => logger.LogInformation("[PerfSeed] {Phase}: {Count} baris dalam {Elapsed:n1} dtk.",
            phaseName, count, stopwatch.Elapsed.TotalSeconds);

    private sealed record ApplicationSeedResult(
        List<BatchApplication> Applications,
        List<ApplicationStage> ApplicationStages);

    private sealed record SessionPlan(
        InterviewSession Session,
        List<StageEmployee> Panel,
        bool IsPast);

    private sealed record SessionSeedResult(
        List<InterviewSession> Sessions,
        List<SessionInterview> SessionInterviews,
        List<InterviewAssessment> Assessments);

    private sealed record TestQuestionTemplate(
        string QuestionText,
        DiscType DiscType,
        List<QuestionOptions> Options);
}
