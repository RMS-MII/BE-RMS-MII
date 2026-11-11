using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace WEBAPI.Templates;

public static class EmailTemplates
{
    private const string DefaultFrontendUrl = "http://localhost:5173";
    private const string DefaultCandidatePortalUrl = "http://localhost:5175";
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    public static string GetFrontendUrl(IConfiguration configuration)
    {
        var configuredUrl = configuration["App:FrontendUrl"]?.Trim().TrimEnd('/');
        return string.IsNullOrWhiteSpace(configuredUrl)
            ? DefaultFrontendUrl
            : configuredUrl;
    }

    public static string GetCandidatePortalUrl(IConfiguration configuration, Guid candidateId)
    {
        var configuredUrl = configuration["App:CandidatePortalUrl"]?.Trim().TrimEnd('/');
        var baseUrl = string.IsNullOrWhiteSpace(configuredUrl) ? DefaultCandidatePortalUrl : configuredUrl;
        return $"{baseUrl}/portal/{candidateId}";
    }

    public static EmailContent StageInvitation(
        string candidateName,
        string batchName,
        string stageName,
        string portalUrl,
        DateTime deadlineUtc)
    {
        return EmailTemplateRenderer.Render(
            "StageInvitation",
            Values(
                ("recipientName", candidateName),
                ("batchName", batchName),
                ("stageName", stageName),
                ("portalUrl", portalUrl),
                ("deadlineText", $"{FormatDateTime(deadlineUtc)} UTC")));
    }

    public static EmailContent CandidateInterviewInvitation(
        string candidateName,
        string stageName,
        string sessionName,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        bool isOnline,
        string? meetingUrl,
        string? location,
        string portalUrl)
    {
        return EmailTemplateRenderer.Render(
            "CandidateInterviewInvitation",
            Values(
                ("recipientName", candidateName),
                ("stageName", stageName),
                ("sessionName", sessionName),
                ("scheduleText", FormatSchedule(scheduledStart, scheduledEnd)),
                ("modeText", InterviewMode(isOnline, meetingUrl, location)),
                ("portalUrl", portalUrl)));
    }

    public static EmailContent InterviewerSchedule(
        string interviewerName,
        string sessionName,
        string candidateName,
        string stageName,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        bool isOnline,
        string? meetingUrl,
        string? location,
        string appUrl)
    {
        return EmailTemplateRenderer.Render(
            "InterviewerSchedule",
            Values(
                ("recipientName", interviewerName),
                ("sessionName", sessionName),
                ("candidateName", candidateName),
                ("stageName", stageName),
                ("scheduleText", FormatSchedule(scheduledStart, scheduledEnd)),
                ("modeText", InterviewMode(isOnline, meetingUrl, location)),
                ("appUrl", appUrl)));
    }

    public static EmailContent InterviewRescheduled(
        string recipientName,
        string sessionName,
        string stageName,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        bool isOnline,
        string? meetingUrl,
        string? location,
        string actionUrl,
        string? reviewNotes)
    {
        var noteText = string.IsNullOrWhiteSpace(reviewNotes)
            ? "Silakan konfirmasi kehadiran Anda sesuai jadwal terbaru."
            : $"Catatan HR: {reviewNotes}";

        return EmailTemplateRenderer.Render(
            "InterviewRescheduled",
            Values(
                ("recipientName", recipientName),
                ("sessionName", sessionName),
                ("stageName", stageName),
                ("scheduleText", FormatSchedule(scheduledStart, scheduledEnd)),
                ("modeText", InterviewMode(isOnline, meetingUrl, location)),
                ("actionUrl", actionUrl),
                ("noteText", noteText)));
    }

    public static EmailContent InterviewerAssignment(
        string interviewerName,
        string stageName,
        string batchName,
        bool isReassignment,
        string appUrl)
    {
        return EmailTemplateRenderer.Render(
            isReassignment ? "InterviewerReassignment" : "InterviewerAssignment",
            Values(
                ("recipientName", interviewerName),
                ("stageName", stageName),
                ("batchName", batchName),
                ("appUrl", appUrl)));
    }

    public static EmailContent TakeHome(
        string candidateName,
        string stageName,
        string taskTitle,
        DateTime deadline,
        string instructions,
        string fileTakeHome,
        string submissionUrl)
    {
        return EmailTemplateRenderer.Render(
            "TakeHome",
            Values(
                ("recipientName", candidateName),
                ("taskTitle", taskTitle),
                ("stageName", stageName),
                ("deadlineText", $"{FormatDateTime(deadline)} WIB"),
                ("instructions", instructions),
                ("fileTakeHome", fileTakeHome),
                ("submissionUrl", submissionUrl)));
    }

    public static EmailContent RescheduleRequestReceived(
        string requesterName,
        string sessionName,
        string stageName,
        DateTime originalStart,
        DateTime originalEnd,
        DateTime proposedStart,
        DateTime proposedEnd,
        string reason)
    {
        return EmailTemplateRenderer.Render(
            "RescheduleRequestReceived",
            Values(
                ("recipientName", requesterName),
                ("sessionName", sessionName),
                ("stageName", stageName),
                ("originalScheduleText", FormatSchedule(originalStart, originalEnd)),
                ("proposedScheduleText", FormatSchedule(proposedStart, proposedEnd)),
                ("reason", reason)));
    }

    public static EmailContent RescheduleResult(
        string sessionName,
        string stageName,
        string decisionText,
        string explanation,
        string? reviewNotes)
    {
        var noteText = string.IsNullOrWhiteSpace(reviewNotes)
            ? string.Empty
            : $"Catatan HR: {reviewNotes}";

        return EmailTemplateRenderer.Render(
            "RescheduleResult",
            Values(
                ("recipientName", string.Empty),
                ("sessionName", sessionName),
                ("stageName", stageName),
                ("decisionText", decisionText),
                ("decisionExplanation", explanation),
                ("noteText", noteText)));
    }

    public static EmailContent StageRejection(
        string candidateName,
        string stageName,
        string batchName,
        string? notes)
    {
        var noteText = string.IsNullOrWhiteSpace(notes)
            ? string.Empty
            : $"Catatan dari HR:\n{notes}";

        return EmailTemplateRenderer.Render(
            "StageRejection",
            Values(
                ("recipientName", candidateName),
                ("stageName", stageName),
                ("batchName", batchName),
                ("noteText", noteText)));
    }

    public static EmailContent StagePassed(
        string candidateName,
        string stageName,
        string nextStageName,
        string batchName,
        string portalUrl,
        DateTime? nextStageDeadlineUtc)
    {
        return EmailTemplateRenderer.Render(
            "StagePassed",
            Values(
                ("recipientName", candidateName),
                ("stageName", stageName),
                ("nextStageName", nextStageName),
                ("batchName", batchName),
                ("portalUrl", portalUrl),
                ("deadlineText", nextStageDeadlineUtc is null
                    ? string.Empty
                    : $"{FormatDateTime(nextStageDeadlineUtc.Value)} UTC"),
                ("nextInstructions", nextStageDeadlineUtc is null
                    ? "Informasi jadwal tahap berikutnya akan disampaikan secara terpisah."
                    : "Silakan buka portal kandidat dan selesaikan tes sebelum batas waktu.")));
    }

    public static EmailContent FinalResultAccepted(string candidateName, string batchName)
    {
        return EmailTemplateRenderer.Render(
            "FinalResultAccepted",
            Values(("recipientName", candidateName), ("batchName", batchName)));
    }

    public static EmailContent FinalResultRejection(
        string candidateName,
        string batchName,
        string? notes)
    {
        var noteText = string.IsNullOrWhiteSpace(notes)
            ? string.Empty
            : $"Catatan dari HR:\n{notes}";

        return EmailTemplateRenderer.Render(
            "FinalResultRejection",
            Values(
                ("recipientName", candidateName),
                ("batchName", batchName),
                ("noteText", noteText)));
    }

    public static EmailContent OfferingLetter(
        string candidateName,
        string batchName,
        string offeringLetterUrl)
    {
        return EmailTemplateRenderer.Render(
            "OfferingLetter",
            Values(
                ("recipientName", candidateName),
                ("batchName", batchName),
                ("offeringLetterUrl", offeringLetterUrl)));
    }

    public static EmailContent InterviewerDeclined(
        string hrName,
        string interviewerName,
        string stageName,
        string batchName,
        string? note)
    {
        return EmailTemplateRenderer.Render(
            "InterviewerDeclined",
            Values(
                ("recipientName", hrName),
                ("interviewerName", interviewerName),
                ("stageName", stageName),
                ("batchName", batchName),
                ("declineReason", string.IsNullOrWhiteSpace(note) ? "-" : note)));
    }

    private static IReadOnlyDictionary<string, string?> Values(
        params (string Key, string? Value)[] values)
    {
        var dictionary = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in values)
            dictionary[key] = value;

        return dictionary;
    }

    private static string FormatDateTime(DateTime value) =>
        value.ToString("dd MMMM yyyy HH:mm", IndonesianCulture);

    private static string FormatSchedule(DateTime start, DateTime end) =>
        $"{FormatDateTime(start)} - {end.ToString("HH:mm", IndonesianCulture)} WIB";

    private static string InterviewMode(bool isOnline, string? meetingUrl, string? location)
    {
        if (isOnline)
        {
            return string.IsNullOrWhiteSpace(meetingUrl)
                ? "Daring"
                : $"Daring melalui tautan meeting: {meetingUrl}";
        }

        return string.IsNullOrWhiteSpace(location)
            ? "Tatap Muka"
            : $"Tatap Muka di lokasi: {location}";
    }
}
