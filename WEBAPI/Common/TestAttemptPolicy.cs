using WEBAPI.Entities;

namespace WEBAPI.Common;

/// <summary>
/// Aturan waktu pengerjaan tes: durasi dihitung dari saat kandidat benar-benar
/// memulai tes (AttemptStartedAt), bukan dari jadwal stage (StartedAt).
/// </summary>
public static class TestAttemptPolicy
{
    /// <summary>
    /// Toleransi jaringan agar submit yang dikirim tepat saat waktu habis masih
    /// dapat diterima.
    /// </summary>
    public static readonly TimeSpan SubmissionGracePeriod = TimeSpan.FromSeconds(60);

    public static DateTime GetDeadline(ApplicationStage stage)
    {
        if (stage.DurationTest is { } duration && duration > TimeSpan.Zero)
        {
            return (stage.AttemptStartedAt ?? stage.StartedAt) + duration;
        }

        // Tanpa durasi khusus, gunakan batas akhir akses sebagai pengaman.
        return stage.EndAt;
    }

    public static bool IsPastDeadline(ApplicationStage stage, DateTime now)
        => now > GetDeadline(stage) + SubmissionGracePeriod;
}
