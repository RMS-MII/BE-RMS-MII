using FluentValidation;

namespace WEBAPI.Features.Interviewers.AssignInterviewers;

public class AssignInterviewersValidator : AbstractValidator<AssignInterviewersCommand>
{
    public AssignInterviewersValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID wajib diisi.");

        RuleFor(x => x.StageId)
            .NotEmpty().WithMessage("Stage ID wajib diisi.");

        RuleFor(x => x.InterviewerEmployeeIds)
            .NotEmpty().WithMessage("Daftar ID pewawancara minimal harus berisi 1 orang.");

        RuleForEach(x => x.InterviewerEmployeeIds)
            .NotEmpty().WithMessage("ID karyawan pewawancara tidak boleh kosong.");
    }
}
