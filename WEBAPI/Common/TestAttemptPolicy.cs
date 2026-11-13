using WEBAPI.Entities;

namespace WEBAPI.Common;

/// <summary>
/// Aturan waktu pengerjaan tes: durasi dihitung dari saat kandidat benar-benar
/// memulai tes (AttemptStartedAt), bukan dari jadwal stage (StartedAt).
/// Batas akhir tahap (EndAt) tetap berlaku selama pengerjaan.
/// </summary>
public static class TestAttemptPolicy
{
    public static DateTime GetDeadline(ApplicationStage stage)
    {
        if (stage.AttemptStartedAt is not { } startedAt ||
            stage.DurationTest is not { } duration || duration <= TimeSpan.Zero)
            return stage.EndAt;

        var attemptDeadline = startedAt + duration;
        return attemptDeadline < stage.EndAt ? attemptDeadline : stage.EndAt;
    }

    /// <summary>
    /// Memeriksa apakah waktu pengerjaan telah melewati batas waktu ketat.
    /// Submit yang dikirim setelah batas waktu wajib ditolak oleh server.
    /// </summary>
    public static bool IsPastDeadline(ApplicationStage stage, DateTime now)
        => now > GetDeadline(stage);
}
