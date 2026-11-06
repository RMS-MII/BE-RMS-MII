using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.QuestionBanks.Queries.GetQuestionBanks;

public sealed record GetQuestionBanksQuery : IRequest<Result<PagedResult<QuestionBankDto>>>
{
    public Guid? DepartmentId { get; init; }
    public Guid? StageTypeId { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
