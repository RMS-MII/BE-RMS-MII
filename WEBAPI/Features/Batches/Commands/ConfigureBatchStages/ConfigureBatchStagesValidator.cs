using FluentValidation;

namespace WEBAPI.Features.Batches.Commands.ConfigureBatchStages;

public sealed class ConfigureBatchStagesValidator : AbstractValidator<ConfigureBatchStagesCommand>
{
    private const int MaxStages = 20;

    public ConfigureBatchStagesValidator()
    {
        RuleFor(x => x.Stages)
            .NotEmpty()
            .Must(stages => stages.Count <= MaxStages)
            .WithMessage($"A batch can have at most {MaxStages} stages.")
            .Must(HaveUniqueNames)
            .WithMessage("Stage names must be unique within a batch.")
            .Must(HaveAscendingStartDates)
            .WithMessage("Stage start dates must be in ascending order.");

        RuleForEach(x => x.Stages)
            .SetValidator(new ConfigureBatchStageItemValidator());
    }

    private static bool HaveUniqueNames(IReadOnlyList<ConfigureBatchStageItem> stages)
    {
        var names = stages
            .Select(stage => stage.StageName?.Trim() ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToList();

        return names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Count;
    }

    private static bool HaveAscendingStartDates(IReadOnlyList<ConfigureBatchStageItem> stages)
    {
        for (var i = 1; i < stages.Count; i++)
        {
            if (stages[i].StartDate < stages[i - 1].StartDate)
                return false;
        }

        return true;
    }
}

public sealed class ConfigureBatchStageItemValidator : AbstractValidator<ConfigureBatchStageItem>
{
    public ConfigureBatchStageItemValidator()
    {
        RuleFor(x => x.StageTypeId)
            .NotEmpty();

        RuleFor(x => x.StageName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PassingScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PassingScore.HasValue);

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
