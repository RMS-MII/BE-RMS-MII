using System.Globalization;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace WEBAPI.Templates;

public static class EmailTemplates
{
    private const string DefaultFrontendUrl = "https://localhost:5173";
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    public static string GetFrontendUrl(IConfiguration configuration)
    {
        var configuredUrl = configuration["App:FrontendUrl"]?.Trim().TrimEnd('/');
        return string.IsNullOrWhiteSpace(configuredUrl)
            ? DefaultFrontendUrl
            : configuredUrl;
    }

    public static EmailContent StageInvitation(
        string candidateName,
        string batchName,
        string stageName,
        string testUrl,
        DateTime deadlineUtc)
    {
        var deadlineText = $"{FormatDateTime(deadlineUtc)} UTC";

        return Build(
            subject: $"Undangan Tahap {stageName} - {batchName}",
            title: "Undangan Tahap Rekrutmen",
            recipientName: candidateName,
            summary: $"Anda diundang mengikuti tahap {stageName} pada batch {batchName}. Silakan masuk ke aplikasi melalui tautan undangan di bawah ini untuk mengerjakan tes.",
            details:
            [
                ("Batch", batchName),
                ("Tahap", stageName),
                ("Batas Waktu", deadlineText)
            ],
            actionLabel: "Mulai Tes Sekarang",
            actionUrl: testUrl,
            note: "Tes hanya dapat dikerjakan satu kali. Pastikan Anda menyelesaikan tes sebelum batas waktu berakhir.",
            closing: "Terima kasih,\nTim Rekrutmen");
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
        return Build(
            subject: $"Undangan Sesi Wawancara - {sessionName}",
            title: "Undangan Sesi Wawancara",
            recipientName: candidateName,
            summary: $"Selamat! Anda diundang untuk mengikuti sesi wawancara pada tahapan '{stageName}'. Silakan masuk ke portal kandidat melalui tautan undangan di bawah ini untuk melihat detail jadwal dan mengonfirmasi kehadiran.",
            details:
            [
                ("Nama Sesi", sessionName),
                ("Tahap", stageName),
                ("Waktu", FormatSchedule(scheduledStart, scheduledEnd)),
                ("Moda", InterviewMode(isOnline, meetingUrl, location))
            ],
            actionLabel: "Buka Portal Kandidat",
            actionUrl: portalUrl,
            note: "Konfirmasi kehadiran Anda melalui portal kandidat sebelum sesi wawancara dimulai.",
            closing: "Salam,\nTim Rekrutmen");
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
        return Build(
            subject: $"Jadwal Sesi Wawancara - {sessionName}",
            title: "Jadwal Sesi Wawancara",
            recipientName: interviewerName,
            summary: "Anda telah dijadwalkan oleh HR untuk menjadi pewawancara pada sesi berikut. Silakan masuk ke aplikasi RMS untuk memeriksa jadwal dan mengonfirmasi kehadiran Anda.",
            details:
            [
                ("Nama Sesi", sessionName),
                ("Kandidat", candidateName),
                ("Tahap", stageName),
                ("Waktu", FormatSchedule(scheduledStart, scheduledEnd)),
                ("Moda", InterviewMode(isOnline, meetingUrl, location))
            ],
            actionLabel: "Masuk ke Aplikasi RMS",
            actionUrl: appUrl,
            note: "Pastikan Anda mengonfirmasi ketersediaan sebelum sesi wawancara berlangsung.",
            closing: "Salam,\nTim Rekrutmen");
    }

    public static EmailContent InterviewerAssignment(
        string interviewerName,
        string stageName,
        string batchName,
        bool isReassignment,
        string appUrl)
    {
        var summary = isReassignment
            ? $"Anda kembali ditugaskan sebagai pewawancara pada tahapan '{stageName}' untuk batch '{batchName}'. Silakan masuk ke aplikasi RMS untuk mengonfirmasi ketersediaan Anda."
            : $"Anda telah ditugaskan sebagai pewawancara pada tahapan '{stageName}' untuk batch '{batchName}'. Silakan masuk ke aplikasi RMS untuk merespons ketersediaan jadwal Anda.";

        return Build(
            subject: $"Undangan Penugasan Pewawancara - {stageName}",
            title: "Undangan Penugasan Pewawancara",
            recipientName: interviewerName,
            summary: summary,
            details:
            [
                ("Batch", batchName),
                ("Tahap", stageName),
                ("Status", isReassignment ? "Penugasan Kembali" : "Penugasan Baru")
            ],
            actionLabel: "Masuk ke Aplikasi RMS",
            actionUrl: appUrl,
            note: "Silakan konfirmasi ketersediaan Anda melalui menu undangan pada aplikasi RMS.",
            closing: "Salam,\nTim Rekrutmen");
    }

    public static EmailContent TakeHome(
        string candidateName,
        string stageName,
        string taskTitle,
        DateTime deadline,
        string instructions,
        string submissionUrl)
    {
        return Build(
            subject: $"Instruksi Take-Home Test: {taskTitle} - {stageName}",
            title: "Instruksi Take-Home Test",
            recipientName: candidateName,
            summary: $"Pewawancara telah menetapkan tugas Take-Home Test untuk tahapan '{stageName}'. Silakan masuk ke portal kandidat melalui tautan undangan di bawah ini untuk mengunggah hasil pengerjaan Anda.",
            details:
            [
                ("Judul Tugas", taskTitle),
                ("Tahap", stageName),
                ("Batas Waktu Pengumpulan", $"{FormatDateTime(deadline)} WIB")
            ],
            actionLabel: "Unggah Tugas",
            actionUrl: submissionUrl,
            note: $"Instruksi Pengerjaan:\n{instructions}",
            closing: "Salam,\nTim Rekrutmen");
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
        return Build(
            subject: $"Permohonan Reschedule Sesi Wawancara - {sessionName}",
            title: "Permohonan Reschedule Diterima",
            recipientName: requesterName,
            summary: $"Permohonan penjadwalan ulang (reschedule) untuk sesi '{sessionName}' pada tahapan '{stageName}' telah kami terima. Tim HR akan segera meninjau permohonan Anda dan memberikan konfirmasi melalui email.",
            details:
            [
                ("Nama Sesi", sessionName),
                ("Tahap", stageName),
                ("Jadwal Semula", FormatSchedule(originalStart, originalEnd)),
                ("Usulan Jadwal Baru", FormatSchedule(proposedStart, proposedEnd)),
                ("Alasan", reason),
                ("Status", "Menunggu Peninjauan HR")
            ],
            closing: "Terima kasih,\nTim Rekrutmen");
    }

    public static EmailContent RescheduleResult(
        string sessionName,
        string stageName,
        bool isApproved,
        string decisionText,
        string explanation,
        string? reviewNotes)
    {
        var note = string.IsNullOrWhiteSpace(reviewNotes)
            ? null
            : $"Catatan HR: {reviewNotes}";

        return Build(
            subject: $"Hasil Peninjauan Reschedule: {decisionText} - {sessionName}",
            title: "Hasil Peninjauan Reschedule",
            recipientName: null,
            summary: $"Berikut adalah hasil peninjauan permohonan reschedule sesi wawancara '{sessionName}' pada tahapan '{stageName}'.",
            details:
            [
                ("Nama Sesi", sessionName),
                ("Tahap", stageName),
                ("Keputusan", decisionText),
                ("Keterangan", isApproved
                    ? "Permohonan reschedule disetujui. Tim HR akan menerbitkan undangan dengan jadwal baru."
                    : "Permohonan reschedule ditolak. Jadwal wawancara semula tetap berlaku.")
            ],
            note: string.IsNullOrWhiteSpace(note) ? explanation : $"{note}\n\n{explanation}",
            closing: "Salam,\nTim Rekrutmen");
    }

    public static EmailContent StageRejection(
        string candidateName,
        string stageName,
        string batchName,
        string? notes)
    {
        return Build(
            subject: $"Hasil Tahap {stageName} - {batchName}",
            title: "Hasil Tahap Rekrutmen",
            recipientName: candidateName,
            summary: $"Kami menginformasikan bahwa Anda belum lolos pada tahap {stageName} untuk batch {batchName}.",
            details:
            [
                ("Batch", batchName),
                ("Tahap", stageName)
            ],
            note: string.IsNullOrWhiteSpace(notes) ? null : $"Catatan dari HR:\n{notes}",
            closing: "Terima kasih atas partisipasi Anda.\n\nTim Rekrutmen");
    }

    public static EmailContent FinalResultRejection(
        string candidateName,
        string batchName,
        string? notes)
    {
        return Build(
            subject: $"Hasil Akhir Rekrutmen - {batchName}",
            title: "Hasil Akhir Rekrutmen",
            recipientName: candidateName,
            summary: $"Setelah melalui seluruh tahap rekrutmen, kami menginformasikan bahwa Anda belum lolos pada batch {batchName}.",
            details:
            [
                ("Batch", batchName)
            ],
            note: string.IsNullOrWhiteSpace(notes) ? null : $"Catatan dari HR:\n{notes}",
            closing: "Terima kasih atas partisipasi Anda.\n\nTim Rekrutmen");
    }

    public static EmailContent OfferingLetter(
        string candidateName,
        string batchName,
        string offeringLetterUrl)
    {
        return Build(
            subject: $"Surat Penawaran - {batchName}",
            title: "Selamat, Anda Diterima!",
            recipientName: candidateName,
            summary: $"Selamat! Anda diterima pada batch {batchName}. Silakan buka surat penawaran melalui tautan di bawah ini dan berikan respons sesuai instruksi.",
            details:
            [
                ("Batch", batchName)
            ],
            actionLabel: "Lihat Surat Penawaran",
            actionUrl: offeringLetterUrl,
            note: "Silakan tinjau surat penawaran dan berikan respons sesuai instruksi.",
            closing: "Terima kasih,\nTim Rekrutmen");
    }

    public static EmailContent InterviewerDeclined(
        string hrName,
        string interviewerName,
        string stageName,
        string batchName,
        string? note)
    {
        return Build(
            subject: $"Penolakan Penugasan Pewawancara - {interviewerName}",
            title: "Penolakan Penugasan Pewawancara",
            recipientName: hrName,
            summary: $"Pewawancara {interviewerName} telah menolak penugasan pada tahapan '{stageName}' (Batch: {batchName}). Silakan tentukan pewawancara pengganti pada sistem RMS.",
            details:
            [
                ("Pewawancara", interviewerName),
                ("Tahap", stageName),
                ("Batch", batchName),
                ("Alasan Penolakan", string.IsNullOrWhiteSpace(note) ? "-" : note)
            ],
            closing: "Salam,\nTim Rekrutmen");
    }

    private static EmailContent Build(
        string subject,
        string title,
        string? recipientName,
        string summary,
        IReadOnlyList<(string Label, string Value)> details,
        string? actionLabel = null,
        string? actionUrl = null,
        string? note = null,
        string closing = "Salam,\nTim Rekrutmen")
    {
        var greeting = string.IsNullOrWhiteSpace(recipientName)
            ? "Halo,"
            : $"Halo {recipientName.Trim()},";

        var textBody = BuildTextBody(greeting, summary, details, actionLabel, actionUrl, note, closing);
        var contentHtml = string.Join(
            string.Empty,
            Paragraph(greeting),
            Paragraph(summary),
            BuildDetails(details),
            BuildButton(actionLabel, actionUrl),
            Paragraph(note),
            Paragraph(closing));

        return new EmailContent(
            subject,
            textBody,
            BuildHtmlLayout(title, contentHtml));
    }

    private static string BuildTextBody(
        string greeting,
        string summary,
        IReadOnlyList<(string Label, string Value)> details,
        string? actionLabel,
        string? actionUrl,
        string? note,
        string closing)
    {
        var lines = new List<string>
        {
            greeting,
            string.Empty,
            summary.Trim()
        };

        if (details.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Rincian:");
            lines.AddRange(details.Select(detail => $"- {detail.Label}: {detail.Value}"));
        }

        if (!string.IsNullOrWhiteSpace(actionLabel) && !string.IsNullOrWhiteSpace(actionUrl))
        {
            lines.Add(string.Empty);
            lines.Add(actionLabel.Trim());
            lines.Add(actionUrl.Trim());
        }

        if (!string.IsNullOrWhiteSpace(note))
        {
            lines.Add(string.Empty);
            lines.Add(note.Trim());
        }

        lines.Add(string.Empty);
        lines.Add(closing.Trim());

        return string.Join("\n", lines);
    }

    private static string BuildDetails(IReadOnlyList<(string Label, string Value)> details)
    {
        if (details.Count == 0)
            return string.Empty;

        var rows = string.Join(
            string.Empty,
            details.Select(detail => $"""
                <tr>
                    <td style="padding:10px 0; border-bottom:1px solid #eef2f7; font-size:14px; color:#6b7280; width:38%; vertical-align:top;">{Encode(detail.Label)}</td>
                    <td style="padding:10px 0; border-bottom:1px solid #eef2f7; font-size:14px; color:#111827; vertical-align:top;">{EncodeMultiline(detail.Value)}</td>
                </tr>
                """));

        return $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:0 0 20px; border-collapse:collapse;">
                <tbody>{rows}</tbody>
            </table>
            """;
    }

    private static string BuildButton(string? label, string? url)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var safeUrl = Encode(url.Trim());

        return $"""
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:8px 0 20px;">
                <tr>
                    <td style="border-radius:8px; background-color:#1d4ed8;">
                        <a href="{safeUrl}" target="_blank" rel="noopener noreferrer" style="display:inline-block; padding:12px 22px; font-size:15px; font-weight:bold; color:#ffffff; text-decoration:none; border-radius:8px;">{Encode(label.Trim())}</a>
                    </td>
                </tr>
            </table>
            <p style="margin:0 0 16px; font-size:12px; line-height:1.6; color:#6b7280;">
                Jika tombol tidak berfungsi, salin tautan berikut:<br />
                <a href="{safeUrl}" target="_blank" rel="noopener noreferrer" style="color:#1d4ed8; word-break:break-all;">{safeUrl}</a>
            </p>
            """;
    }

    private static string Paragraph(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return $"""
            <p style="margin:0 0 16px; font-size:15px; line-height:1.7; color:#374151;">{EncodeMultiline(text.Trim())}</p>
            """;
    }

    private static string BuildHtmlLayout(string title, string contentHtml)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="id">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{{Encode(title)}}</title>
            </head>
            <body style="margin:0; padding:0; background-color:#f3f6fb; font-family:Arial, Helvetica, sans-serif; color:#1f2937;">
                <div style="display:none; max-height:0; overflow:hidden; opacity:0;">{{Encode(title)}}</div>
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f3f6fb; padding:32px 16px;">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px; background-color:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 16px rgba(15,23,42,0.08);">
                                <tr>
                                    <td style="background-color:#1d4ed8; padding:24px 32px;">
                                        <span style="font-size:24px; font-weight:bold; color:#ffffff; letter-spacing:0.5px;">RMS</span>
                                        <span style="font-size:13px; color:#dbeafe; margin-left:8px;">Recruitment Management System</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:32px;">
                                        <h1 style="margin:0 0 20px; font-size:22px; line-height:1.3; color:#0f172a;">{{Encode(title)}}</h1>
                                        {{contentHtml}}
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:20px 32px 28px; border-top:1px solid #e5e7eb;">
                                        <p style="margin:0; font-size:12px; line-height:1.6; color:#6b7280;">Email ini dikirim otomatis oleh RMS. Jangan membalas email ini.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
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

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string EncodeMultiline(string? value) =>
        Encode(value).Replace("\r\n", "\n").Replace("\n", "<br />");
}
