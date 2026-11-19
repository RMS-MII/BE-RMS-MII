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
    private const int PsychologyTestDurationInMinutes = 25;
    private const int ExpectedQuestionCount = 40;

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
                DepartmentId = DataSeeder.DeptTechId,
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

            // Bank sudah ada dan jumlah soal DISC sudah sesuai target -> tidak perlu seed ulang.
            if (bank.Questions.Count == ExpectedQuestionCount &&
                bank.Questions.All(question => question.DiscType != default))
            {
                await context.SaveChangesAsync();
                return;
            }

            // Ganti soal lama jika belum memiliki metadata DISC atau jumlah soal belum lengkap.
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
            // =====================================================
            // ===== D (Dominance) - 10 Soal =======================
            // =====================================================
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

            (DiscType.Dominance,
                "Ketika menghadapi hambatan besar yang menghalangi target kerja, saya cenderung...",
                "Mencari terobosan secara agresif dan fokus pada pencapaian hasil akhir",
                "Menerima keterbatasan situasi dan menyesuaikan target menjadi lebih rendah"),

            (DiscType.Dominance,
                "Dalam rapat pengambilan keputusan yang berjalan terlalu lambat, saya cenderung...",
                "Memotong diskusi yang berlarut-larut dan mendorong tim segera mengambil kesimpulan",
                "Membiarkan diskusi mengalir meskipun memakan waktu lebih lama dari jadwal"),

            (DiscType.Dominance,
                "Saat diberikan proyek baru dengan tingkat ketidakpastian tinggi, saya cenderung...",
                "Langsung mengambil kendali dan berani menanggung risiko demi memulai eksekusi",
                "Menunggu sampai ada panduan lengkap sebelum berani melangkah"),

            (DiscType.Dominance,
                "Ketika melihat proses kerja di tim yang tidak efisien, saya cenderung...",
                "Segera merombak cara kerja tersebut agar target lebih cepat tercapai",
                "Tetap mengikuti cara lama agar tidak menimbulkan ketidaknyamanan di tim"),

            (DiscType.Dominance,
                "Saat bernegosiasi dengan pihak eksternal atau klien yang sulit, saya cenderung...",
                "Bersikap tegas dan mempertahankan posisi untuk mendapatkan hasil terbaik bagi perusahaan",
                "Mengalah pada sebagian besar tuntutan mereka demi menghindari ketegangan"),

            (DiscType.Dominance,
                "Ketika kinerja salah satu anggota tim menghambat pencapaian target bersama, saya cenderung...",
                "Memberikan teguran langsung dan menuntut perbaikan kinerja segera",
                "Membiarkannya sementara waktu dan berharap ia menyadari kesalahannya sendiri"),

            (DiscType.Dominance,
                "Dalam lingkungan kerja yang sangat kompetitif, saya cenderung...",
                "Termotivasi untuk menjadi yang terbaik dan melampaui target yang ditetapkan",
                "Merasa kurang nyaman dan memilih bekerja di luar sorotan kompetisi"),

            // =====================================================
            // ===== I (Influence) - 10 Soal =======================
            // =====================================================
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

            (DiscType.Influence,
                "Ketika suasana kerja di tim terasa jenuh dan kurang bersemangat, saya cenderung...",
                "Mencairkan suasana dengan humor atau obrolan positif untuk membangkitkan energi tim",
                "Tetap diam dan fokus menyelesaikan pekerjaan masing-masing"),

            (DiscType.Influence,
                "Saat menghadiri acara perusahaan atau pertemuan dengan rekan baru, saya cenderung...",
                "Aktif menyapa banyak orang dan memulai percakapan untuk memperluas jaringan",
                "Hanya berbincang dengan rekan kerja yang sudah saya kenal sebelumnya"),

            (DiscType.Influence,
                "Dalam sesi brainstorming atau pencarian solusi kreatif, saya cenderung...",
                "Melontarkan berbagai ide spontan dan memotivasi anggota lain untuk ikut berpendapat",
                "Mendengarkan terlebih dahulu dan baru berbicara jika diminta secara langsung"),

            (DiscType.Influence,
                "Saat harus mengajak tim mengadopsi perubahan budaya kerja baru, saya cenderung...",
                "Menginspirasi mereka dengan gambaran positif tentang manfaat perubahan tersebut",
                "Menyebarkan dokumen instruksi tertulis tanpa melakukan pendekatan personal"),

            (DiscType.Influence,
                "Ketika berhasil mencapai target bersama tim, saya cenderung...",
                "Mengajak tim merayakannya dan memberikan apresiasi secara terbuka kepada rekan-rekan",
                "Menganggapnya sebagai kewajiban biasa dan langsung beralih ke tugas berikutnya"),

            (DiscType.Influence,
                "Saat menyelesaikan masalah yang melibatkan banyak pihak, saya cenderung...",
                "Mengandalkan pendekatan persuasif dan diskusi terbuka untuk mencapai kesepakatan",
                "Mengandalkan komunikasi tertulis via email agar tidak perlu banyak berdiskusi tatap muka"),

            (DiscType.Influence,
                "Ketika bekerja dalam sebuah proyek baru, saya merasa paling bersemangat jika...",
                "Dapat berkolaborasi dengan banyak orang dan bertukar pikiran secara dinamis",
                "Diberikan ruang kerja mandiri tanpa perlu banyak berinteraksi dengan orang lain"),

            // =====================================================
            // ===== S (Steadiness) - 10 Soal ======================
            // =====================================================
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

            (DiscType.Steadiness,
                "Ketika ada anggota tim baru yang masih kesulitan beradaptasi, saya cenderung...",
                "Membimbingnya dengan sabar sampai ia benar-benar memahami alur kerja tim",
                "Meminta ia mempelajari dokumentasi sendiri agar tidak menyita waktu saya"),

            (DiscType.Steadiness,
                "Saat terjadi perbedaan pendapat yang memanas di antara rekan kerja, saya cenderung...",
                "Menjadi penengah yang menenangkan kedua belah pihak demi menjaga keharmonisan tim",
                "Memihak salah satu argumen secara tajam tanpa memedulikan suasana tim"),

            (DiscType.Steadiness,
                "Dalam menjalankan proyek berdurasi panjang, saya cenderung...",
                "Bekerja secara tekun, sabar, dan konsisten mengikuti tahapan yang telah disepakati",
                "Mudah merasa bosan dan ingin segera pindah ke proyek lain yang lebih menantang"),

            (DiscType.Steadiness,
                "Saat atasan memberikan tugas tambahan kepada tim secara tiba-tiba, saya cenderung...",
                "Kooperatif membantu menyusun ulang pembagian tugas agar beban tim tetap seimbang",
                "Menolak secara langsung karena tugas tersebut di luar rencana awal saya"),

            (DiscType.Steadiness,
                "Ketika rekan kerja menyampaikan keluhan tentang kendala pekerjaannya, saya cenderung...",
                "Menjadi pendengar yang empatik dan memberikan dukungan moral yang menenangkan",
                "Menyuruhnya segera berhenti mengeluh dan langsung fokus mencari solusi teknis"),

            (DiscType.Steadiness,
                "Dalam membangun hubungan kerja jangka panjang, saya cenderung...",
                "Mengutamakan kepercayaan, kesetiaan, dan komunikasi yang santun",
                "Bersikap transaksional selama target pekerjaan saat itu selesai"),

            (DiscType.Steadiness,
                "Saat lingkungan kerja sedang mengalami transisi manajemen, saya cenderung...",
                "Menjadi sosok yang menenangkan rekan-rekan dan menjaga stabilitas operasional harian",
                "Ikut menyebarkan spekulasi yang membuat suasana kerja menjadi kurang kondusif"),

            // =====================================================
            // ===== C (Conscientiousness) - 10 Soal ===============
            // =====================================================
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
                "Langsung mengubah pekerjaan mengikuti masukan tanpa evaluasi lebih lanjut"),

            (DiscType.Conscientiousness,
                "Sebelum menyerahkan laporan atau hasil kerja kepada atasan, saya cenderung...",
                "Memeriksa ulang akurasi data dan kerapian dokumen berkali-kali agar bebas dari kesalahan",
                "Langsung mengirimkannya begitu selesai dibuat tanpa melakukan pengecekan ulang"),

            (DiscType.Conscientiousness,
                "Ketika diminta mengambil keputusan penting dalam pekerjaan, saya cenderung...",
                "Mengumpulkan data pendukung dan bukti objektif terlebih dahulu sebelum memutuskan",
                "Mengandalkan intuisi atau perkiraan kasar agar keputusan cepat selesai"),

            (DiscType.Conscientiousness,
                "Saat mengerjakan tugas yang memiliki prosedur operasional standar (SOP) ketat, saya cenderung...",
                "Mematuhi setiap tahapan prosedur secara disiplin demi menjamin kualitas hasil",
                "Melewati beberapa tahapan prosedur agar pekerjaan lebih cepat beres"),

            (DiscType.Conscientiousness,
                "Ketika menemukan kejanggalan kecil pada data atau sistem kerja, saya cenderung...",
                "Menelusuri akar masalahnya secara sistematis hingga tuntas dan terdokumentasi",
                "Mengabaikannya selama belum menimbulkan masalah besar pada operasional saat ini"),

            (DiscType.Conscientiousness,
                "Dalam mengorganisir dokumen dan file pekerjaan sehari-hari, saya cenderung...",
                "Menatanya secara terstruktur dan rapi agar mudah ditelusuri kapan saja",
                "Menyimpannya secara acak selama saya masih ingat kira-kira di mana letaknya"),

            (DiscType.Conscientiousness,
                "Saat mempelajari sistem atau perangkat kerja baru, saya cenderung...",
                "Membaca dokumentasi teknis secara teliti untuk memahami cara kerjanya secara menyeluruh",
                "Langsung mencoba-coba secara acak tanpa membaca panduan penggunaannya"),

            (DiscType.Conscientiousness,
                "Ketika menyusun estimasi waktu dan anggaran sebuah proyek, saya cenderung...",
                "Membuat perhitungan presisi berdasarkan data historis dan skenario kontingensi",
                "Memberikan angka perkiraan kasar tanpa rincian komponen yang jelas")
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