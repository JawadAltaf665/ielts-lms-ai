using FluentValidation;

namespace IELTSLms.Application.Writing.Commands.SubmitWriting;

public sealed class SubmitWritingCommandValidator : AbstractValidator<SubmitWritingCommand>
{
    public SubmitWritingCommandValidator()
    {
        RuleFor(x => x.WritingTaskId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.ResponseText).NotEmpty().WithErrorCode("VALIDATION_WRITING_RESPONSE_TEXT_REQUIRED");
    }
}
