using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.QuestionBanks.Queries.GetQuestionBankById;

public sealed record GetQuestionBankByIdQuery(Guid Id)
    : IRequest<Result<QuestionBankDetailDto>>;
