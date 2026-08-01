using FluentValidation;

namespace IELTSLms.Application.Grammar.CreateGrammarTopic;

public sealed class CreateGrammarTopicCommandValidator : AbstractValidator<CreateGrammarTopicCommand>
{
    public CreateGrammarTopicCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Description).MaximumLength(2000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
