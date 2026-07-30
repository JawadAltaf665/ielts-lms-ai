using FluentValidation;

namespace IELTSLms.Application.Grammar.SubmitGrammarAttempt;

public sealed class SubmitGrammarAttemptCommandValidator : AbstractValidator<SubmitGrammarAttemptCommand>
{
    public SubmitGrammarAttemptCommandValidator()
    {
        RuleFor(x => x.Answers).NotEmpty().WithErrorCode("VALIDATION_GRAMMAR_ANSWERS_REQUIRED");
    }
}
