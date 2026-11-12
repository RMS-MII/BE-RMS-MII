using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

/// <summary>
/// Ringkasan skor DISC untuk satu sifat (D, I, S, C).
/// </summary>
public sealed class DiscSummaryDto
{
    /// <summary>Sifat DISC yang diukur (Dominance, Influence, Steadiness, Conscientiousness).</summary>
    public DiscType Trait { get; set; }

    /// <summary>Jumlah jawaban yang memilih kutub "Most" untuk sifat ini.</summary>
    public int TotalMost { get; set; }

    /// <summary>Jumlah jawaban yang memilih kutub "Least" untuk sifat ini.</summary>
    public int TotalLeast { get; set; }

    /// <summary>Selisih antara TotalMost dan TotalLeast (TotalMost - TotalLeast).</summary>
    public int Difference { get; set; }
}
