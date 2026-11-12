using WEBAPI.Entities;

namespace WEBAPI.Common;

/// <summary>
/// Aturan waktu pengerjaan tes: durasi dihitung dari saat kandidat benar-benar
/// memulai tes (AttemptStartedAt), bukan dari jadwal stage (StartedAt).
/// Batas waktu dihitung dan divalidasi secara strict di sisi server
/// berbasis timestamp server (DateTime.UtcNow).
/// </summary>
public static class TestAttemptPolicy
{
    public static DateTime GetDeadline(ApplicationStage stage)
    {
        if (stage.DurationTest is { } duration && duration > TimeSpan.Zero)
        {
            return (stage.AttemptStartedAt ?? stage.StartedAt) + duration;
        }

        // Tanpa durasi khusus, gunakan batas akhir akses sebagai pengaman.
        return stage.EndAt;
    }

    /// <summary>
    /// Memeriksa apakah waktu pengerjaan telah melewati batas waktu ketat.
    /// Submit yang dikirim setelah batas waktu wajib ditolak oleh server.
    /// </summary>
    public static bool IsPastDeadline(ApplicationStage stage, DateTime now)
        => now > GetDeadline(stage);
}
