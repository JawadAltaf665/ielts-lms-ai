using FluentValidation;

namespace IELTSLms.Application.AI.RequestReadingCoaching;

public sealed class RequestReadingCoachingCommandValidator : AbstractValidator<RequestReadingCoachingCommand>
{
    public RequestReadingCoachingCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(4000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
