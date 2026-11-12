using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

/// <summary>
/// Menghitung skor tes pada skala 0-100 tanpa bergantung pada jumlah soal.
/// Skor mentah kandidat dinormalisasi terhadap skor maksimum seluruh soal tes,
/// sehingga berapa pun jumlah soal nilai maksimumnya tetap 100.
/// </summary>
public static class TestScoreCalculator
{
    /// <summary>Nilai maksimum skor tes setelah normalisasi.</summary>
    public const decimal MaximumScore = 100m;

    /// <summary>
    /// Menghitung skor mentah maksimum yang mungkin diperoleh dari seluruh soal tes.
    /// Soal single-choice memakai skor opsi tertinggi, sedangkan multiple-choice
    /// menjumlahkan seluruh opsi bernilai positif karena semuanya dapat dipilih.
    /// </summary>
    public static decimal CalculateMaxRawScore(IEnumerable<Question> questions)
    {
        decimal maxRaw = 0m;

        foreach (var question in questions)
        {
            var activeOptions = question.QuestionOptions
                .Where(option => option.IsActive)
                .ToList();

            if (activeOptions.Count == 0)
                continue;

            var maxOptionScore = question.QuestionType == QuestionType.SingleChoice
                ? activeOptions.Max(option => option.Score)
                : activeOptions
                    .Where(option => option.Score > 0)
                    .Sum(option => option.Score);

            if (maxOptionScore <= 0)
                continue;

            maxRaw += maxOptionScore * question.Weight;
        }

        return maxRaw;
    }

    /// <summary>
    /// Menormalisasi skor mentah kandidat ke skala 0-100.
    /// Mengembalikan 0 bila tes tidak memiliki skor maksimum yang valid.
    /// </summary>
    public static decimal Normalize(decimal rawEarned, decimal maxRaw)
    {
        if (maxRaw <= 0m)
            return 0m;

        var normalized = rawEarned / maxRaw * MaximumScore;
        normalized = Math.Clamp(normalized, 0m, MaximumScore);

        return Math.Round(normalized, 2, MidpointRounding.AwayFromZero);
    }
}
