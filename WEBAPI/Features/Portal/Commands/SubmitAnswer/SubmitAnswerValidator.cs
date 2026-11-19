using FluentValidation;
using WEBAPI.Features.Portal.Commands.SubmitTest;

namespace WEBAPI.Features.Portal.Commands.SubmitAnswer;

public sealed class SubmitAnswerValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();

        RuleFor(x => x.StageId)
            .NotEmpty();

        RuleFor(x => x.Answers)
            .NotNull();

        RuleFor(x => x.Answers)
            .NotEmpty()
            .WithMessage("At least one answer is required.")
            .When(x => !x.IsDraft && x.DevelopmentPass is null);

        RuleFor(x => x.Answers)
            .Must(HaveUniqueQuestionIds)
            .WithMessage("Each question may be answered only once.")
            .When(x => x.Answers is not null);

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(x => x.QuestionId)
                .NotEmpty();

            answer.RuleFor(x => x.SelectedOptionIds)
                .NotEmpty()
                .WithMessage("Each answer must contain at least one selected option.");

            answer.RuleFor(x => x.SelectedOptionIds)
                .Must(optionIds => optionIds.All(optionId => optionId != Guid.Empty))
                .WithMessage("Selected option ids must not be empty.")
                .When(x => x.SelectedOptionIds is not null);

            answer.RuleFor(x => x.SelectedOptionIds)
                .Must(optionIds => optionIds.Distinct().Count() == optionIds.Count)
                .WithMessage("Selected option ids must be unique per question.")
                .When(x => x.SelectedOptionIds is not null);
        });
    }

    private static bool HaveUniqueQuestionIds(IReadOnlyList<SubmitTestAnswerItem> answers) =>
        answers.Select(answer => answer.QuestionId).Distinct().Count() == answers.Count;
}
