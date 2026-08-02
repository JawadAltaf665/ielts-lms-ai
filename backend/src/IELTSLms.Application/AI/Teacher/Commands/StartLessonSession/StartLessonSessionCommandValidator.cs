using FluentValidation;

namespace IELTSLms.Application.AI.Teacher.Commands.StartLessonSession;

public sealed class StartLessonSessionCommandValidator : AbstractValidator<StartLessonSessionCommand>
{
    public StartLessonSessionCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.Topic)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(500).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
