using FluentValidation;

namespace IELTSLms.Application.Grammar.CreateGrammarExercise;

public sealed class CreateGrammarExerciseCommandValidator : AbstractValidator<CreateGrammarExerciseCommand>
{
    public CreateGrammarExerciseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Order).GreaterThanOrEqualTo(0).WithErrorCode("VALIDATION_GRAMMAR_EXERCISE_ORDER");

        RuleFor(x => x.Questions).NotEmpty().WithErrorCode("VALIDATION_GRAMMAR_EXERCISE_QUESTIONS_REQUIRED");

        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Prompt).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
            question.RuleFor(q => q.AnswerKey).NotEmpty().WithErrorCode("VALIDATION_GRAMMAR_ANSWER_KEY_REQUIRED");
            question.RuleFor(q => q.Sequence).GreaterThanOrEqualTo(0).WithErrorCode("VALIDATION_GRAMMAR_QUESTION_SEQUENCE");
        });
    }
}
