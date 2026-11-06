using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.SetFinalResult;

public sealed record SetFinalResultCommand : IRequest<Result<BatchApplicationDto>>
{
    public Guid ApplicationId { get; init; }
    public FinalResultStatus FinalResultStatus { get; init; }
    public string Notes { get; init; } = string.Empty;
}

/// <summary>
/// Permintaan penetapan hasil akhir kelulusan rekrutmen kandidat.
/// </summary>
public sealed record SetFinalResultRequest
{
    /// <summary>
    /// Status hasil akhir (1 = Accepted, 2 = Rejected).
    /// </summary>
    public FinalResultStatus FinalResultStatus { get; init; }

    /// <summary>
    /// Catatan atau ringkasan hasil akhir rekrutmen.
    /// </summary>
    /// <example>Kandidat dinyatakan lolos seluruh rangkaian seleksi dan siap ditawarkan offering letter.</example>
    public string Notes { get; init; } = string.Empty;
}
