using FluentValidation;

namespace IELTSLms.Application.Grammar.UpdateGrammarTopic;

public sealed class UpdateGrammarTopicCommandValidator : AbstractValidator<UpdateGrammarTopicCommand>
{
    public UpdateGrammarTopicCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Description).MaximumLength(2000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
