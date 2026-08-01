using FluentValidation;

namespace IELTSLms.Application.Writing.Commands.CreateWritingTask;

public sealed class CreateWritingTaskCommandValidator : AbstractValidator<CreateWritingTaskCommand>
{
    public CreateWritingTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PromptText).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.MinWordCount).GreaterThan(0).WithErrorCode("VALIDATION_WRITING_WORD_COUNT_BOUNDS_INVALID");

        RuleFor(x => x.MaxWordCount)
            .GreaterThan(0).WithErrorCode("VALIDATION_WRITING_WORD_COUNT_BOUNDS_INVALID")
            .GreaterThanOrEqualTo(x => x.MinWordCount).WithErrorCode("VALIDATION_WRITING_WORD_COUNT_BOUNDS_INVALID");
    }
}
