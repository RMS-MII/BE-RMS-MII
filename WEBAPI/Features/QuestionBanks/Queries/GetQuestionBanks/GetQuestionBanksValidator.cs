using FluentValidation;

namespace WEBAPI.Features.QuestionBanks.Queries.GetQuestionBanks;

public sealed class GetQuestionBanksValidator : AbstractValidator<GetQuestionBanksQuery>
{
    public GetQuestionBanksValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page minimal bernilai 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize harus antara 1 sampai 100.");
    }
}
