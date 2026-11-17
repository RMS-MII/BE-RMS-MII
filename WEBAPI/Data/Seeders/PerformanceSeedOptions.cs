namespace WEBAPI.Data.Seeders;

/// <summary>
/// Konfigurasi seeder data volume untuk pengujian performa.
/// Semua nilai dapat di-override melalui section <c>Seed:Performance</c> di appsettings.
/// </summary>
public sealed class PerformanceSeedOptions
{
    public const string SectionName = "Seed:Performance";

    /// <summary>Aktifkan seeder volume secara otomatis saat startup.</summary>
    public bool Enabled { get; set; }

    /// <summary>Hapus data perf lama sebelum membuat data baru (idempoten).</summary>
    public bool ResetBeforeSeed { get; set; } = true;

    /// <summary>Gunakan SqlBulkCopy untuk tabel volume besar.</summary>
    public bool UseSqlBulkCopy { get; set; } = true;

    /// <summary>Jumlah baris per batch insert.</summary>
    public int BatchSize { get; set; } = 1_000;

    /// <summary>Jumlah employee/akun pewawancara tambahan.</summary>
    public int Interviewers { get; set; } = 30;

    /// <summary>Jumlah batch rekrutmen yang dibuat.</summary>
    public int Batches { get; set; } = 14;

    /// <summary>Jumlah kandidat unik.</summary>
    public int Candidates { get; set; } = 3_000;

    /// <summary>Jumlah lamaran per kandidat (maksimal sebanyak batch non-draft).</summary>
    public int ApplicationsPerCandidate { get; set; } = 3;

    /// <summary>Jumlah log email yang dibuat.</summary>
    public int EmailLogs { get; set; } = 10_000;

    /// <summary>Jumlah permintaan reschedule yang dibuat.</summary>
    public int RescheduleRequests { get; set; } = 500;

    /// <summary>Proporsi tahap wawancara yang memiliki sesi wawancara (0..1).</summary>
    public double InterviewSessionRatio { get; set; } = 0.35;

    /// <summary>Proporsi tahap tes DISC yang memiliki jawaban (0..1).</summary>
    public double CandidateAnswerRatio { get; set; } = 0.60;

    /// <summary>Seed RNG agar data deterministik antar-run.</summary>
    public int RandomSeed { get; set; } = 20261117;

    /// <summary>Bersihkan data perf juga saat reset checkpoint dijalankan.</summary>
    public bool ClearOnCheckpointReset { get; set; } = true;

    public void Normalize()
    {
        BatchSize = Math.Clamp(BatchSize, 100, 20_000);
        Interviewers = Math.Clamp(Interviewers, 1, 1_000);
        Batches = Math.Clamp(Batches, 1, 500);
        Candidates = Math.Clamp(Candidates, 1, 1_000_000);
        ApplicationsPerCandidate = Math.Clamp(ApplicationsPerCandidate, 1, Math.Max(1, Batches));
        EmailLogs = Math.Clamp(EmailLogs, 0, 5_000_000);
        RescheduleRequests = Math.Clamp(RescheduleRequests, 0, 1_000_000);
        InterviewSessionRatio = Math.Clamp(InterviewSessionRatio, 0d, 1d);
        CandidateAnswerRatio = Math.Clamp(CandidateAnswerRatio, 0d, 1d);
    }
}
