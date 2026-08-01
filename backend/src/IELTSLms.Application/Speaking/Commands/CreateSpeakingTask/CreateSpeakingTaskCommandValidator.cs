using FluentValidation;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;

public sealed class CreateSpeakingTaskCommandValidator : AbstractValidator<CreateSpeakingTaskCommand>
{
    public CreateSpeakingTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PromptText).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.Part).IsInEnum().WithErrorCode("VALIDATION_REQUIRED");
    }
}
