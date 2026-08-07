using FluentValidation;

namespace IELTSLms.Application.AI.RequestVocabularyCoaching;

public sealed class RequestVocabularyCoachingCommandValidator : AbstractValidator<RequestVocabularyCoachingCommand>
{
    public RequestVocabularyCoachingCommandValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Topic).MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");
        RuleFor(x => x.Difficulty).MaximumLength(50).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
