using Microsoft.EntityFrameworkCore;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Data.Seeders;

/// <summary>
/// Seeder untuk bank soal dan soal tes psikologi (Forced-Choice / DISC).
/// Setiap butir soal memiliki dua pilihan jawaban (A/B) dengan skor binary:
/// pilihan A merepresentasikan kutub "Most" (skor 10) dan pilihan B "Least" (skor 0).
/// Setiap soal diberi <see cref="DiscType"/> agar jawaban dapat diagregasi per sifat D/I/S/C.
/// </summary>
public static class PsychologyTestSeeder
{
    private const int PsychologyTestDurationInMinutes = 15;

    public static async Task SeedAsync(RmsDbContext context)
    {
        // Hapus bank soal psikotes duplikat (bukan bank utama) agar soal & durasi tidak ambigu.
        await RemoveDuplicatePsychologyBanksAsync(context);

        var bank = await context.QuestionBanks
            .Include(questionBank => questionBank.Questions)
                .ThenInclude(question => question.QuestionOptions)
            .FirstOrDefaultAsync(questionBank => questionBank.Id == DataSeeder.QBankPsikotesId);

        if (bank is null)
        {
            bank = new QuestionBank
            {
                Id = DataSeeder.QBankPsikotesId,
                DepartmentId = DataSeeder.DeptHrId,
                StageTypeId = DataSeeder.StageTypePsikotesId,
                CreatedBy = DataSeeder.EmpHrManagerId,
                DurationInMinutes = PsychologyTestDurationInMinutes,
                CreatedAt = DateTime.UtcNow
            };

            context.QuestionBanks.Add(bank);
        }
        else
        {
            bank.DurationInMinutes = PsychologyTestDurationInMinutes;

            // Bank sudah ada dan soal DISC sudah lengkap -> tidak perlu seed ulang.
            if (bank.Questions.Count > 0 && bank.Questions.All(question => question.DiscType != default))
            {
                await context.SaveChangesAsync();
                return;
            }

            // Ganti soal lama (belum memiliki metadata DISC) dengan soal DISC baru.
            if (bank.Questions.Count > 0)
            {
                var legacyOptions = bank.Questions
                    .SelectMany(question => question.QuestionOptions)
                    .ToList();

                context.QuestionOptions.RemoveRange(legacyOptions);
                context.Questions.RemoveRange(bank.Questions);
                await context.SaveChangesAsync();
            }
        }

        var questions = new List<Question>();
        var options = new List<QuestionOptions>();

        AddDiscQuestions(bank.Id, questions, options);

        context.Questions.AddRange(questions);
        context.QuestionOptions.AddRange(options);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Menghapus bank soal psikotes lain (selain bank utama) agar hanya ada satu bank
    /// soal DISC per stage type. Aman dilakukan karena jawaban kandidat menyimpan snapshot
    /// teks soal/opsi dan tipe DISC, sehingga tidak lagi bergantung pada data master soal.
    /// </summary>
    private static async Task RemoveDuplicatePsychologyBanksAsync(RmsDbContext context)
    {
        var duplicateBanks = await context.QuestionBanks
            .Include(questionBank => questionBank.Questions)
                .ThenInclude(question => question.QuestionOptions)
            .Where(questionBank =>
                questionBank.Id != DataSeeder.QBankPsikotesId &&
                questionBank.StageTypeId == DataSeeder.StageTypePsikotesId)
            .ToListAsync();

        if (duplicateBanks.Count == 0)
            return;

        foreach (var duplicateBank in duplicateBanks)
        {
            context.QuestionOptions.RemoveRange(
                duplicateBank.Questions.SelectMany(question => question.QuestionOptions));
            context.Questions.RemoveRange(duplicateBank.Questions);
            context.QuestionBanks.Remove(duplicateBank);
        }

        await context.SaveChangesAsync();
    }

    private static void AddDiscQuestions(Guid bankId, List<Question> questions, List<QuestionOptions> options)
    {
        var data = new (DiscType DiscType, string QuestionText, string OptionAText, string OptionBText)[]
        {
            // ===== D (Dominance) =====
            (DiscType.Dominance,
                "Saat tim menghadapi deadline yang sangat ketat, saya cenderung...",
                "Mengambil inisiatif dan mengarahkan tim untuk menyelesaikan pekerjaan lebih cepat",
                "Menunggu arahan dan mengikuti ritme kerja tim yang sudah berjalan"),

            (DiscType.Dominance,
                "Ketika terjadi konflik internal dalam tim, saya cenderung...",
                "Mengambil peran kepemimpinan untuk menentukan solusi akhir",
                "Menghindari keterlibatan dan berharap konflik selesai dengan sendirinya"),

            (DiscType.Dominance,
                "Saat harus memilih prioritas proyek yang saling bertentangan, saya cenderung...",
                "Mengambil keputusan tegas berdasarkan prioritas bisnis yang ditetapkan",
                "Menunda keputusan sampai ada arahan dari atasan"),

            // ===== I (Influence) =====
            (DiscType.Influence,
                "Saat harus memperkenalkan ide baru kepada tim, saya cenderung...",
                "Mengomunikasikannya dengan antusias dan meyakinkan orang lain untuk mendukung",
                "Menyampaikan ide secara singkat lalu menunggu tanggapan tanpa mendorong diskusi"),

            (DiscType.Influence,
                "Dalam kerja sama lintas divisi, saya cenderung...",
                "Membangun hubungan baik dan aktif bernegosiasi dengan pihak lain",
                "Membatasi interaksi hanya pada kebutuhan pekerjaan yang bersifat formal"),

            (DiscType.Influence,
                "Saat presentasi di depan pemangku kepentingan, saya cenderung...",
                "Tampil percaya diri dan interaktif untuk menarik perhatian audiens",
                "Fokus pada data dan menghindari interaksi tambahan dengan audiens"),

            // ===== S (Steadiness) =====
            (DiscType.Steadiness,
                "Saat rekan kerja mengalami tekanan pekerjaan, saya cenderung...",
                "Mendengarkan dan membantu meringankan beban mereka",
                "Fokus pada tugas saya sendiri agar target pribadi tetap tercapai"),

            (DiscType.Steadiness,
                "Ketika terjadi perubahan mendadak dalam rencana kerja, saya cenderung...",
                "Menjaga ketenangan dan membantu tim beradaptasi secara bertahap",
                "Mengubah cara kerja secara spontan tanpa banyak berkoordinasi dengan tim"),

            (DiscType.Steadiness,
                "Dalam menyelesaikan tugas rutin, saya cenderung...",
                "Menjaga konsistensi dan mendukung ritme kerja tim yang stabil",
                "Mencari perubahan cara kerja meskipun cara yang ada masih berjalan baik"),

            // ===== C (Conscientiousness) =====
            (DiscType.Conscientiousness,
                "Saat memeriksa hasil pekerjaan tim, saya cenderung...",
                "Memastikan setiap detail sesuai standar dan prosedur yang berlaku",
                "Menerima hasil apa adanya selama target utama sudah tercapai"),

            (DiscType.Conscientiousness,
                "Dalam perencanaan strategi jangka panjang, saya cenderung...",
                "Menyusun rencana rinci dengan analisis risiko yang mendalam",
                "Mengikuti rencana umum tanpa banyak detail teknis"),

            (DiscType.Conscientiousness,
                "Saat menerima masukan kritis dari pengguna atau stakeholder, saya cenderung...",
                "Menganalisis dampak masukan secara menyeluruh sebelum melakukan perubahan",
                "Langsung mengubah pekerjaan mengikuti masukan tanpa evaluasi lebih lanjut")
        };

        for (var i = 0; i < data.Length; i++)
        {
            var questionId = Guid.NewGuid();

            questions.Add(new Question
            {
                Id = questionId,
                QuestionBankId = bankId,
                QuestionText = data[i].QuestionText,
                QuestionType = QuestionType.SingleChoice,
                DiscType = data[i].DiscType,
                Weight = 1.0m,
                SequenceNumber = i + 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            options.Add(new QuestionOptions
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                OptionLabel = "A",
                OptionText = data[i].OptionAText,
                Score = 10.0m,
                SequenceNumber = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            options.Add(new QuestionOptions
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                OptionLabel = "B",
                OptionText = data[i].OptionBText,
                Score = 0.0m,
                SequenceNumber = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
