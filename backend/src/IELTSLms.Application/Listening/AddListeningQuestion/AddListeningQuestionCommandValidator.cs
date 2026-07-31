using FluentValidation;

namespace IELTSLms.Application.Listening.AddListeningQuestion;

public sealed class AddListeningQuestionCommandValidator : AbstractValidator<AddListeningQuestionCommand>
{
    public AddListeningQuestionCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.AnswerKey).NotEmpty().WithErrorCode("VALIDATION_LISTENING_QUESTION_ANSWER_KEY_REQUIRED");
        RuleFor(x => x.Sequence).GreaterThanOrEqualTo(0).WithErrorCode("VALIDATION_LISTENING_QUESTION_SEQUENCE");
    }
}
