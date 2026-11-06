using FluentValidation;

namespace WEBAPI.Features.StageTypes.Commands.CreateStageType;

public sealed class CreateStageTypeValidator : AbstractValidator<CreateStageTypeCommand>
{
    public CreateStageTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama tipe tahapan wajib diisi.")
            .MaximumLength(30).WithMessage("Nama tipe tahapan maksimal 30 karakter.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Deskripsi maksimal 100 karakter.");

        RuleFor(x => x.StageCategory)
            .IsInEnum().WithMessage("Kategori tahapan tidak valid.");
    }
}

