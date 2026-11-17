using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Seeders;

/// <summary>
/// Factory data deterministik untuk seeder volume.
/// Tidak menyentuh database sehingga mudah dipakai ulang oleh seeder lain.
/// </summary>
internal static class PerformanceDataGenerator
{
    private static readonly string[] FirstNames =
    [
        "Adi", "Agus", "Aisyah", "Andi", "Anggi", "Anisa", "Arif", "Bagus", "Bayu", "Bella",
        "Bima", "Cahya", "Citra", "Dedi", "Dewi", "Dimas", "Dinda", "Eka", "Fajar", "Farah",
        "Fauzi", "Fitri", "Galih", "Gita", "Hadi", "Hana", "Hendra", "Indah", "Intan", "Irfan",
        "Joko", "Kartika", "Laila", "Lukman", "Maya", "Nabila", "Nanda", "Nina", "Putra", "Rani",
        "Rizky", "Sari", "Satria", "Sinta", "Taufik", "Tiara", "Wahyu", "Wulan", "Yudi", "Zaki"
    ];

    private static readonly string[] LastNames =
    [
        "Pratama", "Saputra", "Lestari", "Wijaya", "Santoso", "Rahmawati", "Nugroho", "Hidayat",
        "Kusuma", "Permata", "Siregar", "Simanjuntak", "Hutapea", "Nasution", "Lubis", "Halim",
        "Setiawan", "Gunawan", "Hartono", "Susanto", "Maulana", "Ramadhan", "Firmansyah", "Akbar",
        "Firdaus", "Anugrah", "Wibowo", "Suryana", "Kurniawan", "Yuliana", "Anggraini", "Puspita",
        "Maharani", "Oktaviani", "Salsabila", "Aulia", "Ningsih", "Safitri", "Ramadhani", "Prasetyo",
        "Handoko", "Sudirman", "Tanjung", "Manurung", "Sihombing", "Panggabean", "Sitompul",
        "Nainggolan", "Panjaitan", "Simbolon"
    ];

    private static readonly string[] EducationLevels =
    [
        "SMA/SMK", "D3 Teknik Informatika", "S1 Teknik Informatika", "S1 Sistem Informasi",
        "S1 Manajemen", "S2 Teknik Informatika", "S1 Akuntansi"
    ];

    private static readonly string[] Institutions =
    [
        "Universitas Indonesia", "Institut Teknologi Bandung", "Universitas Gadjah Mada",
        "Institut Teknologi Sepuluh Nopember", "Universitas Brawijaya", "Universitas Diponegoro",
        "Universitas Padjadjaran", "Universitas Airlangga", "Universitas Bina Nusantara",
        "Universitas Telkom", "Universitas Gunadarma", "Politeknik Negeri Jakarta"
    ];

    private static readonly string[] Addresses =
    [
        "Jl. Sudirman No. 1, Jakarta", "Jl. Gatot Subroto No. 12, Jakarta",
        "Jl. Asia Afrika No. 8, Bandung", "Jl. Pemuda No. 45, Surabaya",
        "Jl. Malioboro No. 20, Yogyakarta", "Jl. Diponegoro No. 77, Semarang",
        "Jl. Basuki Rahmat No. 5, Malang", "Jl. Ahmad Yani No. 33, Bekasi",
        "Jl. Raya Bogor No. 99, Depok", "Jl. Gajah Mada No. 10, Denpasar",
        "Jl. Merdeka No. 17, Medan", "Jl. Pettarani No. 21, Makassar"
    ];

    private static readonly string[] JobTitles =
    [
        "Software Engineer", "Senior Software Engineer", "QA Engineer", "QA Lead",
        "HR Specialist", "Finance Analyst", "Engineering Manager", "Product Manager",
        "Business Analyst", "DevOps Engineer"
    ];

    public static IReadOnlyList<Candidate> CreateCandidates(int count, DateTime now)
    {
        var candidates = new List<Candidate>(count);

        for (var index = 0; index < count; index++)
        {
            candidates.Add(new Candidate
            {
                Id = Guid.CreateVersion7(),
                IsInTalentPool = index % 10 != 0,
                FullName = CandidateName(index),
                Gender = index % 2 == 0 ? Gender.Male : Gender.Female,
                Email = CandidateEmail(index),
                PhoneNumber = $"0812{index:D7}",
                BirthDate = new DateOnly(1988 + index % 12, (index % 12) + 1, (index % 27) + 1),
                Cv = index % 4 == 0 ? null : $"perf/cv/candidate-{index:D6}.pdf",
                Portfolio = index % 5 == 0 ? $"https://portfolio.example.test/{index:D6}" : null,
                Education = EducationLevels[index % EducationLevels.Length],
                Institution = Institutions[index % Institutions.Length],
                Address = Addresses[index % Addresses.Length],
                ExpectedSalary = 6_000_000m + index % 40 * 250_000m,
                Gpa = 2.75m + index % 126 * 0.01m,
                CreatedAt = now.AddDays(-(index % 365)).AddMinutes(-(index % 1_440))
            });
        }

        return candidates;
    }

    public static Employee CreatePerfEmployee(int index, Guid departmentId, DateTime now)
        => new()
        {
            Id = Guid.CreateVersion7(),
            DepartmentId = departmentId,
            ManagerId = null,
            EmployeeNumber = $"PERF-EMP-{index + 1:D3}",
            FullName = $"Perf Interviewer {index + 1:D3}",
            Email = PerfEmployeeEmail(index),
            Gender = index % 2 == 0 ? Gender.Male : Gender.Female,
            JobTitle = JobTitles[index % JobTitles.Length],
            IsActive = true,
            CreatedAt = now.AddDays(-(index % 180)).AddMinutes(-(index % 720))
        };

    public static Account CreatePerfAccount(
        Guid employeeId,
        Guid roleId,
        int index,
        string passwordHash,
        DateTime now)
        => new()
        {
            Id = Guid.CreateVersion7(),
            EmployeeId = employeeId,
            RoleId = roleId,
            Username = $"perf.emp.{index + 1:D3}",
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = now.AddDays(-(index % 180)).AddMinutes(-(index % 720))
        };

    /// <summary>
    /// Membuat pasangan unik (kandidat, batch) untuk lamaran. Setiap kandidat
    /// memperoleh <paramref name="applicationsPerCandidate"/> batch berbeda.
    /// </summary>
    public static IReadOnlyList<(int CandidateIndex, int BatchIndex)> CreateApplicationPairs(
        int candidateCount,
        int batchCount,
        int applicationsPerCandidate)
    {
        if (candidateCount <= 0 || batchCount <= 0)
            return [];

        var perCandidate = Math.Clamp(applicationsPerCandidate, 1, batchCount);
        var stride = FindCoprimeStride(batchCount);
        var pairs = new List<(int CandidateIndex, int BatchIndex)>(candidateCount * perCandidate);

        for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
        {
            var start = candidateIndex % batchCount;
            for (var offset = 0; offset < perCandidate; offset++)
            {
                var batchIndex = (start + offset * stride) % batchCount;
                pairs.Add((candidateIndex, batchIndex));
            }
        }

        return pairs;
    }

    public static IReadOnlyList<EmailLog> CreateEmailLogs(
        int count,
        IReadOnlyList<Candidate> candidates,
        IReadOnlyList<Employee> employees,
        DateTime now,
        Random random)
    {
        if (count <= 0 || candidates.Count == 0)
            return [];

        var logs = new List<EmailLog>(count);

        for (var index = 0; index < count; index++)
        {
            var type = (EmailType)(index % 11 + 1);
            var attemptedAt = now.AddDays(-(index % 365)).AddMinutes(-(index % 720));
            var statusRoll = random.Next(100);
            var status = statusRoll < 82
                ? EmailDeliveryStatus.Sent
                : statusRoll < 94
                    ? EmailDeliveryStatus.Failed
                    : EmailDeliveryStatus.Pending;

            var useEmployeeRecipient = employees.Count > 0 && index % 5 == 0;
            var recipientEmail = useEmployeeRecipient
                ? employees[index % employees.Count].Email
                : candidates[index % candidates.Count].Email;

            logs.Add(new EmailLog
            {
                Id = Guid.CreateVersion7(),
                SenderEmail = "no-reply@rms.local",
                RecipientEmail = recipientEmail,
                Type = type,
                Subject = $"PERF {type.ToDisplayName()} #{index:D6}",
                Body = $"Email simulasi untuk pengujian performa RMS. Tipe: {type}. " +
                       $"Penerima: {recipientEmail}. Tidak ada aksi yang diperlukan.",
                HtmlBody = index % 5 == 0
                    ? $"<p>Email simulasi performa RMS untuk <b>{recipientEmail}</b>.</p>"
                    : null,
                Status = status,
                AttemptedAt = attemptedAt,
                SentAt = status == EmailDeliveryStatus.Sent
                    ? attemptedAt.AddSeconds(1 + random.Next(20))
                    : null,
                ErrorMessage = status == EmailDeliveryStatus.Failed
                    ? "Simulated SMTP failure during performance seeding."
                    : null
            });
        }

        return logs;
    }

    public static string CandidateEmail(int index) => $"perf.cand.{index:D6}@example.test";

    public static string PerfEmployeeEmail(int index) => $"perf.emp.{index + 1:D3}@rms.local";

    private static string CandidateName(int index)
    {
        var firstName = FirstNames[index % FirstNames.Length];
        var lastName = LastNames[index / FirstNames.Length % LastNames.Length];
        return $"{firstName} {lastName}";
    }

    private static int FindCoprimeStride(int batchCount)
    {
        if (batchCount <= 2)
            return 1;

        for (var candidate = 3; candidate < batchCount; candidate++)
        {
            if (GreatestCommonDivisor(candidate, batchCount) == 1)
                return candidate;
        }

        return 1;
    }

    private static int GreatestCommonDivisor(int left, int right)
    {
        while (right != 0)
        {
            var remainder = left % right;
            left = right;
            right = remainder;
        }

        return left;
    }
}
