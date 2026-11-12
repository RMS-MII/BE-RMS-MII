using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

public interface ITestAttemptFinalizer
{
    /// <summary>
    /// Menyelesaikan attempt tes: menjumlahkan skor jawaban yang berhasil tersimpan,
    /// menormalisasinya ke skala 0-100, lalu menetapkan status akhir tahap.
    /// </summary>
    Task<ApplicationStageStatus> FinalizeAsync(
        ApplicationStage stage,
        DateTime now,
        CancellationToken cancellationToken);
}
