using FluentValidation;

namespace IELTSLms.Application.Speaking.Commands.SubmitSpeakingAttempt;

public sealed class SubmitSpeakingAttemptCommandValidator : AbstractValidator<SubmitSpeakingAttemptCommand>
{
    public SubmitSpeakingAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
    }
}
