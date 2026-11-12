using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

public interface ITestAttemptFinalizer
{
    /// <summary>
    /// Menyelesaikan attempt tes: menjumlahkan skor jawaban yang berhasil tersimpan
    /// dan menetapkan status akhir tahap berdasarkan hasil perhitungan skor.
    /// </summary>
    Task<ApplicationStageStatus> FinalizeAsync(
        ApplicationStage stage,
        DateTime now,
        CancellationToken cancellationToken);
}
