using FluentValidation;

namespace IELTSLms.Application.AI.Prompts.CreatePromptVersion;

public sealed class CreatePromptVersionCommandValidator : AbstractValidator<CreatePromptVersionCommand>
{
    public CreatePromptVersionCommandValidator()
    {
        RuleFor(x => x.Feature).NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(100).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Scope).MaximumLength(50).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.SystemTemplate).NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(20_000).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
