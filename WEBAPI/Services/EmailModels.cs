using WEBAPI.Templates;

namespace WEBAPI.Services;

public sealed record BulkEmailMessage(
    string RecipientEmail,
    EmailContent Content);

public sealed record EmailSendResult(
    string RecipientEmail,
    bool Success,
    string? ErrorMessage = null);
