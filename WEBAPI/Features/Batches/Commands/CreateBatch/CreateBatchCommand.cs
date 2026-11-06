using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.CreateBatch;

/// <summary>
/// Data pembuatan batch rekrutmen baru.
/// </summary>
public sealed record CreateBatchCommand : IRequest<Result<RecruitmentBatchDto>>
{
    /// <summary>
    /// ID Departemen tujuan batch (diperoleh dari GET /api/departments).
    /// </summary>
    /// <example>01930002-0000-7000-8000-000000000002</example>
    public Guid DepartmentId { get; init; }

    /// <summary>
    /// Kode unik batch. Opsional; jika dikosongkan/null, sistem otomatis men-generate dengan format: BATCH-{DEPT}-{YEAR}-{001}.
    /// </summary>
    /// <example>BATCH-SE-2026-001</example>
    public string? BatchCode { get; init; }

    /// <summary>
    /// Nama judul batch rekrutmen.
    /// </summary>
    /// <example>Backend .NET Developer Batch 1</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Deskripsi singkat mengenai lowongan/batch rekrutmen ini.
    /// </summary>
    /// <example>Perekrutan talenta Backend Engineer kuartal pertama 2026</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Tanggal dan waktu mulai batch.
    /// </summary>
    /// <example>2026-11-10T00:00:00Z</example>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Tanggal dan waktu berakhir batch (harus setelah StartDate).
    /// </summary>
    /// <example>2026-12-31T23:59:59Z</example>
    public DateTime EndDate { get; init; }
}
