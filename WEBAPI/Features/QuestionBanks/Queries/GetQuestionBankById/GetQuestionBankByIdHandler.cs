using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.QuestionBanks.Queries.GetQuestionBankById;

public sealed class GetQuestionBankByIdHandler(RmsDbContext db)
    : IRequestHandler<GetQuestionBankByIdQuery, Result<QuestionBankDetailDto>>
{
    public async Task<Result<QuestionBankDetailDto>> Handle(
        GetQuestionBankByIdQuery request, CancellationToken ct)
    {
        var questionBank = await db.QuestionBanks
            .AsNoTracking()
            .Include(questionBank => questionBank.Department)
            .Include(questionBank => questionBank.StageType)
            .Include(questionBank => questionBank.Questions)
                .ThenInclude(question => question.QuestionOptions)
            .FirstOrDefaultAsync(questionBank => questionBank.Id == request.Id, ct);

        if (questionBank is null)
        {
            return Result<QuestionBankDetailDto>.Failure(
                Error.NotFound($"Bank soal dengan ID '{request.Id}' tidak ditemukan."));
        }

        return Result<QuestionBankDetailDto>.Success(questionBank.ToDetailDto());
    }
}
