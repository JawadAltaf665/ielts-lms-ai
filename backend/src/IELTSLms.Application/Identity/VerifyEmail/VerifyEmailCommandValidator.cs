using FluentValidation;

namespace IELTSLms.Application.Identity.VerifyEmail;

/// <summary>Presence validation for <see cref="VerifyEmailCommand"/>.</summary>
public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("A verification token is required.");
    }
}
