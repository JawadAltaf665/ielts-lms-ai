using FluentValidation;

namespace IELTSLms.Application.AI.RequestListeningCoaching;

public sealed class RequestListeningCoachingCommandValidator : AbstractValidator<RequestListeningCoachingCommand>
{
    public RequestListeningCoachingCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(4000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
