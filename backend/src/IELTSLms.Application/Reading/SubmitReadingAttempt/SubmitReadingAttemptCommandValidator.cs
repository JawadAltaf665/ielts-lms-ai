using FluentValidation;

namespace IELTSLms.Application.Reading.SubmitReadingAttempt;

public sealed class SubmitReadingAttemptCommandValidator : AbstractValidator<SubmitReadingAttemptCommand>
{
    public SubmitReadingAttemptCommandValidator()
    {
        RuleFor(x => x.Answers).NotEmpty().WithErrorCode("VALIDATION_READING_ANSWERS_REQUIRED");
    }
}
