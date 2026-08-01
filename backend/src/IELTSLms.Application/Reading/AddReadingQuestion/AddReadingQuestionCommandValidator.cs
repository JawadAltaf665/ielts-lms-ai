using FluentValidation;

namespace IELTSLms.Application.Reading.AddReadingQuestion;

public sealed class AddReadingQuestionCommandValidator : AbstractValidator<AddReadingQuestionCommand>
{
    public AddReadingQuestionCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.AnswerKey).NotEmpty().WithErrorCode("VALIDATION_READING_QUESTION_ANSWER_KEY_REQUIRED");
        RuleFor(x => x.Sequence).GreaterThanOrEqualTo(0).WithErrorCode("VALIDATION_READING_QUESTION_SEQUENCE");
    }
}
