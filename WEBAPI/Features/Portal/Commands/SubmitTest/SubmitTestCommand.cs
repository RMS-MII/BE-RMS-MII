using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Portal.Commands.SubmitTest;

public sealed record SubmitTestCommand : IRequest<Result<SubmitTestResultDto>>
{
    public Guid CandidateId { get; init; }
    public Guid StageId { get; init; }
    public IReadOnlyList<SubmitTestAnswerItem> Answers { get; init; } = [];
}

/// <summary>
/// Permintaan pengiriman jawaban tes daring oleh kandidat.
/// </summary>
public sealed record SubmitTestRequest
{
    /// <summary>
    /// Daftar jawaban untuk setiap butir soal ujian.
    /// </summary>
    public IReadOnlyList<SubmitTestAnswerItem> Answers { get; init; } = [];
}

public sealed record SubmitTestAnswerItem
{
    /// <summary>
    /// ID Butir Soal.
    /// </summary>
    public Guid QuestionId { get; init; }

    /// <summary>
    /// Daftar ID pilihan jawaban yang dipilih kandidat.
    /// </summary>
    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = [];
}
