using FluentValidation;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.GenerateQuestionVariants;

public sealed class GenerateQuestionVariantsCommandValidator : AbstractValidator<GenerateQuestionVariantsCommand>
{
    public GenerateQuestionVariantsCommandValidator()
    {
        RuleFor(x => x.SourceQuestionId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.DifficultyDelta).InclusiveBetween(-3, 3).WithErrorCode("VALIDATION_OUT_OF_RANGE");
    }
}
