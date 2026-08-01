using FluentValidation;

namespace IELTSLms.Application.Listening.SubmitListeningAttempt;

public sealed class SubmitListeningAttemptCommandValidator : AbstractValidator<SubmitListeningAttemptCommand>
{
    public SubmitListeningAttemptCommandValidator()
    {
        RuleFor(x => x.Answers).NotEmpty().WithErrorCode("VALIDATION_LISTENING_ANSWERS_REQUIRED");
    }
}
