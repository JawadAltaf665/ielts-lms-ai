using FluentValidation;

namespace IELTSLms.Application.AI.RequestGrammarCoaching;

public sealed class RequestGrammarCoachingCommandValidator : AbstractValidator<RequestGrammarCoachingCommand>
{
    public RequestGrammarCoachingCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(4000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
