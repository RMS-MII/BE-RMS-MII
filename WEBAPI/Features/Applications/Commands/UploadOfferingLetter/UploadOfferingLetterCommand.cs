using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.UploadOfferingLetter;

public sealed record UploadOfferingLetterCommand : IRequest<Result<BatchApplicationDto>>
{
    public Guid ApplicationId { get; init; }
    public string FileOfferingLetter { get; init; } = string.Empty;
}

/// <summary>
/// Permintaan unggah berkas surat penawaran kerja (Offering Letter).
/// </summary>
public sealed record UploadOfferingLetterRequest
{
    /// <summary>
    /// Tautan URL berkas surat penawaran kerja (Google Drive / Cloud Storage).
    /// </summary>
    /// <example>https://drive.google.com/file/d/offering-letter-kandidat/view</example>
    public string FileOfferingLetter { get; init; } = string.Empty;
}
