using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Features.Portal.Commands.SubmitTest;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.SubmitAnswer;

public sealed record SubmitAnswerCommand : IRequest<Result<SubmitAnswerResultDto>>
{
    public Guid CandidateId { get; init; }
    public Guid StageId { get; init; }
    public IReadOnlyList<SubmitTestAnswerItem> Answers { get; init; } = [];
}

/// <summary>
/// Permintaan pengiriman jawaban tes daring oleh kandidat.
/// Validasi keabsahan batas waktu wajib dilakukan di server.
/// </summary>
public sealed record SubmitAnswerRequest
{
    /// <summary>
    /// Daftar jawaban untuk setiap butir soal ujian.
    /// </summary>
    public IReadOnlyList<SubmitTestAnswerItem> Answers { get; init; } = [];
}
