using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.ConfigureBatchStages;

public sealed record ConfigureBatchStagesCommand : IRequest<Result<IReadOnlyCollection<BatchStageDto>>>
{
    public Guid BatchId { get; init; }
    public IReadOnlyList<ConfigureBatchStageItem> Stages { get; init; } = [];
}

/// <summary>
/// Permintaan konfigurasi susunan tahapan batch seleksi.
/// </summary>
public sealed record ConfigureBatchStagesRequest
{
    /// <summary>
    /// Daftar tahapan seleksi yang disusun berurutan.
    /// </summary>
    public IReadOnlyList<ConfigureBatchStageItem> Stages { get; init; } = [];
}

public sealed record ConfigureBatchStageItem
{
    /// <summary>
    /// ID Tipe Tahapan (diperoleh dari GET /api/stage-types).
    /// </summary>
    public Guid StageTypeId { get; init; }

    /// <summary>
    /// Nama khusus tahapan untuk batch ini.
    /// </summary>
    /// <example>Technical Assessment .NET</example>
    public string StageName { get; init; } = string.Empty;

    /// <summary>
    /// Apakah tahapan ini aktif.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Nilai batas kelulusan (passing score) opsional.
    /// </summary>
    /// <example>75.0</example>
    public decimal? PassingScore { get; init; }

    /// <summary>
    /// Tanggal dan waktu mulai tahapan.
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Tanggal dan waktu berakhir tahapan.
    /// </summary>
    public DateTime EndDate { get; init; }
}
