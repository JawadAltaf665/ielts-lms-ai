using FluentValidation;

namespace IELTSLms.Application.AI.Prompts.RollbackPromptVersion;

public sealed class RollbackPromptVersionCommandValidator : AbstractValidator<RollbackPromptVersionCommand>
{
    public RollbackPromptVersionCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(500).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
