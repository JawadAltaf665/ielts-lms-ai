using FluentValidation;

namespace IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;

public sealed class CreateSentenceExerciseCommandValidator : AbstractValidator<CreateSentenceExerciseCommand>
{
    public CreateSentenceExerciseCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().WithErrorCode("VALIDATION_SENTENCE_EXERCISE_PROMPT_REQUIRED");
        RuleFor(x => x.TargetPattern).NotEmpty().WithErrorCode("VALIDATION_SENTENCE_EXERCISE_TARGET_PATTERN_REQUIRED");
    }
}
