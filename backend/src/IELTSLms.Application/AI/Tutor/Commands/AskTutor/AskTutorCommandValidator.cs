using FluentValidation;

namespace IELTSLms.Application.AI.Tutor.Commands.AskTutor;

public sealed class AskTutorCommandValidator : AbstractValidator<AskTutorCommand>
{
    public AskTutorCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(4000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
