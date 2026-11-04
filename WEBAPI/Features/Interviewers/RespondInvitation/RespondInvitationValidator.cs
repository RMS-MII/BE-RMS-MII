using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.Interviewers.RespondInvitation;

public class RespondInvitationValidator : AbstractValidator<RespondInvitationCommand>
{
    public RespondInvitationValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEmpty()
            .WithMessage("ID undangan penugasan pewawancara wajib diisi.");

        RuleFor(x => x.Response)
            .Must(r => r == StageEmployeeStatus.Accepted || r == StageEmployeeStatus.Declined)
            .WithMessage("Respon hanya boleh bernilai 'Accepted' (Bersedia) atau 'Declined' (Menolak).");

        RuleFor(x => x.Note)
            .NotEmpty()
            .When(x => x.Response == StageEmployeeStatus.Declined)
            .WithMessage("Alasan penolakan (Note) wajib diisi jika menolak penugasan.")
            .MaximumLength(500)
            .WithMessage("Catatan tidak boleh lebih dari 500 karakter.");
    }
}
