using System.Net;
using System.Text.RegularExpressions;

namespace WEBAPI.Templates;

public static class EmailTemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{(\w+)\}\}",
        RegexOptions.Compiled);

    public static EmailContent Render(
        string templateName,
        IReadOnlyDictionary<string, string?> values)
    {
        var definition = EmailTemplateCatalog.Get(templateName);

        var subject = Replace(definition.Subject, values).Trim();
        var title = Replace(definition.Title, values).Trim();
        var greeting = NormalizeGreeting(Replace(definition.Greeting, values).Trim());
        var summary = Replace(definition.Summary, values).Trim();

        var details = new List<(string Label, string Value)>();
        foreach (var detail in definition.Details ?? [])
        {
            var label = Replace(detail.Label, values).Trim();
            var value = Replace(detail.Value, values).Trim();

            if (string.IsNullOrWhiteSpace(label))
                continue;

            if (detail.SkipWhenEmpty && string.IsNullOrWhiteSpace(value))
                continue;

            details.Add((label, value));
        }

        var actionLabel = Replace(definition.ActionLabel, values).Trim();
        var actionUrl = Replace(definition.ActionUrl, values).Trim();
        if (string.IsNullOrWhiteSpace(actionLabel) || string.IsNullOrWhiteSpace(actionUrl))
        {
            actionLabel = null;
            actionUrl = null;
        }

        var note = Replace(definition.Note, values).Trim();
        if (string.IsNullOrWhiteSpace(note))
            note = null;

        var closing = Replace(definition.Closing, values).Trim();
        if (string.IsNullOrWhiteSpace(closing))
            closing = "Salam,\nTim Rekrutmen";

        if (string.IsNullOrWhiteSpace(subject))
            throw new InvalidOperationException(
                $"Email template '{templateName}' produced an empty subject.");

        var textBody = BuildTextBody(greeting, summary, details, actionLabel, actionUrl, note, closing);

        var contentHtml = string.Join(
            string.Empty,
            new[]
            {
                Paragraph(greeting),
                Paragraph(summary),
                BuildDetails(details),
                BuildButton(actionLabel, actionUrl),
                Paragraph(note),
                Paragraph(closing)
            });

        return new EmailContent(
            subject,
            textBody,
            EmailTemplateCatalog.RenderLayout(title, contentHtml));
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
            summary
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
            lines.Add(actionLabel);
            lines.Add(actionUrl);
        }

        if (!string.IsNullOrWhiteSpace(note))
        {
            lines.Add(string.Empty);
            lines.Add(note);
        }

        lines.Add(string.Empty);
        lines.Add(closing);

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

    private static string Replace(
        string? text,
        IReadOnlyDictionary<string, string?> values)
    {
        if (text is null)
            return string.Empty;

        return PlaceholderRegex.Replace(text, match =>
        {
            var key = match.Groups[1].Value;
            return values.TryGetValue(key, out var value)
                ? value ?? string.Empty
                : string.Empty;
        });
    }

    private static string NormalizeGreeting(string greeting)
    {
        var normalized = Regex.Replace(greeting, @"[ \t]{2,}", " ");
        normalized = Regex.Replace(normalized, @"[ \t]+,", ",");
        return normalized.Trim();
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string EncodeMultiline(string? value) =>
        Encode(value).Replace("\r\n", "\n").Replace("\n", "<br />");
}
