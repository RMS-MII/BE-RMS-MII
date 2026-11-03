using System.ComponentModel.DataAnnotations;

namespace WEBAPI.Entities.Enums;

#region Enum Definitions

public enum Gender
{
    Male = 1,
    Female = 2
}

public enum StageCategory
{
    Test = 1,
    Interview = 2
}

public enum QuestionType
{
    SingleChoice = 1,
    MultipleChoice = 2
}

public enum BatchStatus
{
    Draft = 1,
    Open = 2,
    Closed = 3
}

public enum OfferingLetterStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Negotiate = 4
}

public enum ApplicationStatus
{
    InProcess = 1,
    Rejected = 2,
    Dropped = 3,
    Accepted = 4
}

public enum FinalResultStatus
{
    Accepted = 1,
    Rejected = 2
}

public enum ApplicationStageStatus
{
    InProgress = 1,
    Submitted = 2,
    Expired = 3,
    Prioritized = 4,
    Considered = 5,
    Rejected = 6,
    Dropped = 7
}

public enum StageEmployeeStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3
}

public enum SessionInterviewStatus
{
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum InterviewSessionStatus
{
    Scheduled = 1,
    PendingConfirmation = 2,
    Rescheduled = 3,
    AcceptedByInterviewer = 4,
    AcceptedByCandidate = 5
}

public enum Recommendation
{
    StronglyRecommend = 1,
    Recommend = 2,
    Consider = 3,
    NotRecommend = 4
}

public enum RescheduleStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum EmailType
{
    StageInvitation = 1,
    InterviewerInvitation = 2,
    InterviewInvitation = 3,
    RescheduleRequest = 4,
    RescheduleResult = 5,
    TakeHome = 6,
    Rejection = 7,
    FinalResult = 8
}

#endregion

/// <summary>
/// Kelas terpusat penyedia metode-metode pembantu untuk semua Enum sistem RMS:
/// - Parsing dari string/int
/// - Format dan label representasi tampilan (Display Name)
/// - Validasi keberadaan nilai enum
/// - Aturan transisi status bisnis (Batch, Application, Reschedule, Interview, Offering Letter, Final Result)
/// - Konversi daftar pilihan (Options list) untuk dropdown DTO/Frontend
/// </summary>
public static class EnumHelper
{
    #region Generic Helper Methods

    /// <summary>
    /// Mem-parsing string atau int menjadi nilai enum T secara aman (case-insensitive).
    /// </summary>
    public static bool TryParseEnum<T>(string? value, out T result) where T : struct, Enum
    {
        return Enum.TryParse(value, true, out result) && Enum.IsDefined(typeof(T), result);
    }

    /// <summary>
    /// Memeriksa apakah nilai integer valid dan terdefinisi dalam enum T.
    /// </summary>
    public static bool IsDefined<T>(int value) where T : struct, Enum
    {
        return Enum.IsDefined(typeof(T), value);
    }

    /// <summary>
    /// Mengambil semua nilai enum T beserta nama string dan nilai integer-nya.
    /// </summary>
    public static IReadOnlyList<(int Value, string Name)> GetValuesAndNames<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Select(e => (Convert.ToInt32(e), e.ToString()))
            .ToList();
    }

    #endregion

    #region Display & Label Formatting

    public static string ToDisplayName(this Gender gender) => gender switch
    {
        Gender.Male => "Laki-laki",
        Gender.Female => "Perempuan",
        _ => gender.ToString()
    };

    public static string ToDisplayName(this StageCategory category) => category switch
    {
        StageCategory.Test => "Tes Daring",
        StageCategory.Interview => "Wawancara",
        _ => category.ToString()
    };

    public static string ToDisplayName(this QuestionType type) => type switch
    {
        QuestionType.SingleChoice => "Pilihan Tunggal",
        QuestionType.MultipleChoice => "Pilihan Ganda",
        _ => type.ToString()
    };

    public static string ToDisplayName(this BatchStatus status) => status switch
    {
        BatchStatus.Draft => "Draft",
        BatchStatus.Open => "Buka",
        BatchStatus.Closed => "Tutup",
        _ => status.ToString()
    };

    public static string ToDisplayName(this OfferingLetterStatus status) => status switch
    {
        OfferingLetterStatus.Pending => "Menunggu Keputusan",
        OfferingLetterStatus.Approved => "Diterima oleh Kandidat",
        OfferingLetterStatus.Rejected => "Ditolak oleh Kandidat",
        OfferingLetterStatus.Negotiate => "Negosiasi",
        _ => status.ToString()
    };

    public static string ToDisplayName(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.InProcess => "Sedang Diproses",
        ApplicationStatus.Rejected => "Ditolak",
        ApplicationStatus.Dropped => "Gugur",
        ApplicationStatus.Accepted => "Diterima",
        _ => status.ToString()
    };

    public static string ToDisplayName(this FinalResultStatus status) => status switch
    {
        FinalResultStatus.Accepted => "Lolos / Diterima",
        FinalResultStatus.Rejected => "Tidak Lolos / Ditolak",
        _ => status.ToString()
    };

    public static string ToDisplayName(this ApplicationStageStatus status) => status switch
    {
        ApplicationStageStatus.InProgress => "Sedang Berjalan",
        ApplicationStageStatus.Submitted => "Telah Dikirim",
        ApplicationStageStatus.Expired => "Kedaluwarsa",
        ApplicationStageStatus.Prioritized => "Diprioritaskan",
        ApplicationStageStatus.Considered => "Ditetapkan / Dipertimbangkan",
        ApplicationStageStatus.Rejected => "Ditolak",
        ApplicationStageStatus.Dropped => "Gugur",
        _ => status.ToString()
    };

    public static string ToDisplayName(this StageEmployeeStatus status) => status switch
    {
        StageEmployeeStatus.Pending => "Menunggu Konfirmasi",
        StageEmployeeStatus.Accepted => "Bersedia",
        StageEmployeeStatus.Declined => "Menolak",
        _ => status.ToString()
    };

    public static string ToDisplayName(this SessionInterviewStatus status) => status switch
    {
        SessionInterviewStatus.InProgress => "Sedang Berlangsung",
        SessionInterviewStatus.Completed => "Selesai",
        SessionInterviewStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };

    public static string ToDisplayName(this InterviewSessionStatus status) => status switch
    {
        InterviewSessionStatus.Scheduled => "Dijadwalkan",
        InterviewSessionStatus.PendingConfirmation => "Menunggu Konfirmasi",
        InterviewSessionStatus.Rescheduled => "Dijadwal Ulang",
        InterviewSessionStatus.AcceptedByInterviewer => "Dikonfirmasi Pewawancara",
        InterviewSessionStatus.AcceptedByCandidate => "Dikonfirmasi Kandidat",
        _ => status.ToString()
    };

    public static string ToDisplayName(this Recommendation rec) => rec switch
    {
        Recommendation.StronglyRecommend => "Sangat Direkomendasikan",
        Recommendation.Recommend => "Direkomendasikan",
        Recommendation.Consider => "Dipertimbangkan",
        Recommendation.NotRecommend => "Tidak Direkomendasikan",
        _ => rec.ToString()
    };

    public static string ToDisplayName(this RescheduleStatus status) => status switch
    {
        RescheduleStatus.Pending => "Menunggu Persetujuan",
        RescheduleStatus.Approved => "Disetujui",
        RescheduleStatus.Rejected => "Ditolak",
        _ => status.ToString()
    };

    public static string ToDisplayName(this EmailType type) => type switch
    {
        EmailType.StageInvitation => "Undangan Tahapan Tes/Wawancara",
        EmailType.InterviewerInvitation => "Undangan Penugasan Pewawancara",
        EmailType.InterviewInvitation => "Undangan Sesi Wawancara Kandidat",
        EmailType.RescheduleRequest => "Permohonan Reschedule",
        EmailType.RescheduleResult => "Hasil Keputusan Reschedule",
        EmailType.TakeHome => "Instruksi Take-Home Test",
        EmailType.Rejection => "Surat Penolakan",
        EmailType.FinalResult => "Pengumuman Hasil Akhir & Offering",
        _ => type.ToString()
    };

    #endregion

    #region Business Logic & Transition Rules

    /// <summary>
    /// Memeriksa keabsahan transisi status Batch rekrutmen.
    /// Alur: Draft -> Open -> Closed.
    /// </summary>
    public static bool CanTransitionTo(this BatchStatus current, BatchStatus target)
    {
        if (current == target) return true;
        return (current, target) switch
        {
            (BatchStatus.Draft, BatchStatus.Open) => true,
            (BatchStatus.Open, BatchStatus.Closed) => true,
            _ => false
        };
    }

    /// <summary>
    /// Memeriksa apakah status aplikasi kandidat sudah berstatus final (selesai).
    /// </summary>
    public static bool IsFinal(this ApplicationStatus status) =>
        status is ApplicationStatus.Accepted or ApplicationStatus.Rejected or ApplicationStatus.Dropped;

    /// <summary>
    /// Memeriksa transisi status lamaran kandidat (BatchApplications).
    /// </summary>
    public static bool CanTransitionTo(this ApplicationStatus current, ApplicationStatus target)
    {
        if (current == target) return true;
        if (current.IsFinal()) return false; // Sekali final tidak dapat diubah kembali

        return (current, target) switch
        {
            (ApplicationStatus.InProcess, ApplicationStatus.Accepted) => true,
            (ApplicationStatus.InProcess, ApplicationStatus.Rejected) => true,
            (ApplicationStatus.InProcess, ApplicationStatus.Dropped) => true,
            _ => false
        };
    }

    /// <summary>
    /// Memeriksa apakah status tahap aplikasi (ApplicationStage) termasuk kategori lolos/memenuhi syarat lanjut.
    /// </summary>
    public static bool IsPassingStage(this ApplicationStageStatus status) =>
        status is ApplicationStageStatus.Prioritized or ApplicationStageStatus.Considered;

    /// <summary>
    /// Memeriksa apakah status tahap aplikasi sudah berstatus final untuk tahapan tersebut.
    /// </summary>
    public static bool IsTerminalStage(this ApplicationStageStatus status) =>
        status is ApplicationStageStatus.Prioritized or ApplicationStageStatus.Considered
               or ApplicationStageStatus.Rejected or ApplicationStageStatus.Dropped or ApplicationStageStatus.Expired;

    /// <summary>
    /// Memeriksa transisi status permintaan reschedule (hanya bisa diubah jika masih Pending).
    /// </summary>
    public static bool CanReview(this RescheduleStatus current, RescheduleStatus decision)
    {
        if (current != RescheduleStatus.Pending) return false;
        return decision is RescheduleStatus.Approved or RescheduleStatus.Rejected;
    }

    /// <summary>
    /// Memeriksa respons kandidat atas Offering Letter.
    /// Dari Pending -> Approved, Rejected, atau Negotiate.
    /// Dari Negotiate -> Approved atau Rejected.
    /// </summary>
    public static bool CanRespondOffering(this OfferingLetterStatus current, OfferingLetterStatus response)
    {
        return (current, response) switch
        {
            (OfferingLetterStatus.Pending, OfferingLetterStatus.Approved) => true,
            (OfferingLetterStatus.Pending, OfferingLetterStatus.Rejected) => true,
            (OfferingLetterStatus.Pending, OfferingLetterStatus.Negotiate) => true,
            (OfferingLetterStatus.Negotiate, OfferingLetterStatus.Approved) => true,
            (OfferingLetterStatus.Negotiate, OfferingLetterStatus.Rejected) => true,
            _ => false
        };
    }

    /// <summary>
    /// Memeriksa apakah status sesi wawancara aktif / valid untuk dinilai.
    /// </summary>
    public static bool IsReadyForAssessment(this SessionInterviewStatus status) =>
        status is SessionInterviewStatus.InProgress or SessionInterviewStatus.Completed;

    #endregion
}
