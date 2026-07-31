using FluentValidation;

namespace IELTSLms.Application.Reading.CreateReadingPassage;

public sealed class CreateReadingPassageCommandValidator : AbstractValidator<CreateReadingPassageCommand>
{
    public CreateReadingPassageCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PassageText).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.DurationSeconds).GreaterThan(0).WithErrorCode("VALIDATION_READING_DURATION_INVALID");
    }
}
