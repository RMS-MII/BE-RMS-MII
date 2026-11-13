using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Services;

/// <summary>
/// Menghitung ringkasan DISC (D/I/S/C) dari jawaban kandidat.
/// Setiap jawaban menyimpan snapshot <see cref="CandidateAnswer.DiscType"/> dan skor mentah,
/// sehingga agregasi tidak bergantung pada data master soal.
/// </summary>
public static class DiscSummaryCalculator
{
    private static readonly DiscType[] Traits =
    [
        DiscType.Dominance,
        DiscType.Influence,
        DiscType.Steadiness,
        DiscType.Conscientiousness
    ];

    /// <summary>
    /// Untuk setiap sifat DISC dihitung jumlah jawaban "Most" (skor &gt; 0),
    /// jumlah jawaban "Least" (skor = 0), dan selisihnya (Most - Least).
    /// </summary>
    public static IReadOnlyCollection<DiscSummaryDto> Summarize(
        IEnumerable<CandidateAnswer> candidateAnswers)
    {
        var answers = candidateAnswers as IReadOnlyCollection<CandidateAnswer>
            ?? candidateAnswers.ToList();

        return Traits
            .Select(trait =>
            {
                var traitAnswers = answers
                    .Where(answer => answer.DiscType == trait)
                    .ToList();

                var totalMost = traitAnswers.Count(answer => answer.Score > 0);
                var totalLeast = traitAnswers.Count(answer => answer.Score == 0);

                return new DiscSummaryDto
                {
                    Trait = trait,
                    TotalMost = totalMost,
                    TotalLeast = totalLeast,
                    Difference = totalMost - totalLeast
                };
            })
            .ToList();
    }
}
