using FluentValidation;

namespace WEBAPI.Features.Interviewers.AssignInterviewers;

public class AssignInterviewersValidator : AbstractValidator<AssignInterviewersCommand>
{
    public AssignInterviewersValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required.");

        RuleFor(x => x.StageId)
            .NotEmpty().WithMessage("Stage ID is required.");

        RuleFor(x => x.InterviewerEmployeeIds)
            .NotEmpty().WithMessage("At least one interviewer ID is required.");

        RuleForEach(x => x.InterviewerEmployeeIds)
            .NotEmpty().WithMessage("Interviewer employee ID must not be empty.");
    }
}
