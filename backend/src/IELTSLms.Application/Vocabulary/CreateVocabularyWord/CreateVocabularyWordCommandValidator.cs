using FluentValidation;

namespace IELTSLms.Application.Vocabulary.CreateVocabularyWord;

public sealed class CreateVocabularyWordCommandValidator : AbstractValidator<CreateVocabularyWordCommand>
{
    public CreateVocabularyWordCommandValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(100).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Sense)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Meaning)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(2000).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Topic).MaximumLength(100).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
