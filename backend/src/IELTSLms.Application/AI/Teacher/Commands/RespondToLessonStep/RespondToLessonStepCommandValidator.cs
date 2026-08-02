using FluentValidation;

namespace IELTSLms.Application.AI.Teacher.Commands.RespondToLessonStep;

public sealed class RespondToLessonStepCommandValidator : AbstractValidator<RespondToLessonStepCommand>
{
    public RespondToLessonStepCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(4000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
