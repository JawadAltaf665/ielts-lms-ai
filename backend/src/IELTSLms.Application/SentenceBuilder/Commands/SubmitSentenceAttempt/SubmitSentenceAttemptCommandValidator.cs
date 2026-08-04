using FluentValidation;

namespace IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;

public sealed class SubmitSentenceAttemptCommandValidator : AbstractValidator<SubmitSentenceAttemptCommand>
{
    public SubmitSentenceAttemptCommandValidator()
    {
        RuleFor(x => x.SubmittedText)
            .NotEmpty().WithErrorCode("VALIDATION_SENTENCE_ATTEMPT_TEXT_REQUIRED")
            .MaximumLength(2000).WithErrorCode("VALIDATION_SENTENCE_ATTEMPT_TEXT_TOO_LONG");
    }
}
