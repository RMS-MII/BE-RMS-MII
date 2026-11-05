namespace WEBAPI.Templates;

public sealed record EmailContent(
    string Subject,
    string TextBody,
    string? HtmlBody = null);
