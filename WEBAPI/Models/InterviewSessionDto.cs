using WEBAPI.Entities.Enums;

namespace WEBAPI.Models;

/// <summary>
/// Permintaan penjadwalan sesi wawancara untuk kandidat.
/// </summary>
public class ScheduleInterviewSessionRequest
{
    /// <summary>
    /// Nama atau judul sesi wawancara.
    /// </summary>
    /// <example>Wawancara User - Dimas Aditya</example>
    public string SessionName { get; set; } = string.Empty;

    /// <summary>
    /// Waktu mulai sesi wawancara.
    /// </summary>
    /// <example>2026-11-20T09:00:00Z</example>
    public DateTime ScheduledStart { get; set; }

    /// <summary>
    /// Waktu selesai sesi wawancara.
    /// </summary>
    /// <example>2026-11-20T10:00:00Z</example>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>
    /// Apakah wawancara daring/online (default: true).
    /// </summary>
    public bool IsOnline { get; set; } = true;

    /// <summary>
    /// Lokasi wawancara fisik jika offline.
    /// </summary>
    /// <example>Ruang Meeting Lantai 3</example>
    public string? Location { get; set; }

    /// <summary>
    /// Tautan pertemuan daring (Google Meet / Zoom / MS Teams).
    /// </summary>
    /// <example>https://meet.google.com/abc-defg-hij</example>
    public string? MeetingUrl { get; set; }

    /// <summary>
    /// Daftar ID Karyawan pewawancara yang ditugaskan pada sesi ini (harus yang sudah Accepted).
    /// </summary>
    public List<Guid>? EmployeeIds { get; set; }

    /// <summary>
    /// Alias dari EmployeeIds (keduanya didukung oleh backend).
    /// </summary>
    public List<Guid>? InterviewerEmployeeIds { get; set; }

    public List<Guid> GetInterviewerEmployeeIds()
    {
        if (EmployeeIds is { Count: > 0 }) return EmployeeIds;
        if (InterviewerEmployeeIds is { Count: > 0 }) return InterviewerEmployeeIds;
        return [];
    }
}

public class InterviewSessionDto
{
    public Guid Id { get; set; }
    public Guid ApplicationStageId { get; set; }
    public Guid BatchStageId { get; set; }
    public Guid RecruitmentBatchId { get; set; }
    public Guid CandidateId { get; set; }
    public string CodeApplication { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public InterviewSessionStatus Status { get; set; }
    public string StatusName => Status.ToDisplayName();
    public List<ScheduledInterviewerDto> Interviewers { get; set; } = [];
    public bool HasPendingReschedule { get; set; }
    public Guid? PendingRescheduleId { get; set; }
    public DateTime? ProposedStart { get; set; }
    public DateTime? ProposedEnd { get; set; }
    public string? RescheduleReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignTakeHomeRequest
{
    public DateTime EndedSubmissionAt { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public class AssignTakeHomeResponse
{
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public DateTime EndedSubmissionAt { get; set; }
    public bool EmailSent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SubmissionDto
{
    public Guid InterviewSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public bool HasTakeHome { get; set; }
    public DateTime? EndedSubmissionAt { get; set; }
    public bool IsSubmitted { get; set; }
    public string? FileSubmission { get; set; }
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
}
