using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;

public sealed record EvaluateStageCommand : IRequest<Result<ApplicationStageDto>>
{
    public Guid StageId { get; init; }
    public ApplicationStageStatus Status { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Permintaan evaluasi kelulusan tahapan seleksi kandidat.
/// </summary>
public sealed record EvaluateStageRequest
{
    /// <summary>
    /// Status keputusan evaluasi, termasuk Dropped untuk kandidat yang tidak melanjutkan stage.
    /// </summary>
    public ApplicationStageStatus Status { get; init; }

    /// <summary>
    /// Catatan evaluasi atau pertimbangan kelulusan (opsional).
    /// </summary>
    /// <example>Kandidat memenuhi seluruh kualifikasi dan nilai batas minimum.</example>
    public string? Notes { get; init; }
}
