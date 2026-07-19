using FluentValidation;

namespace IELTSLms.Application.Identity.ForgotPassword;

/// <summary>Presence validation for <see cref="ForgotPasswordCommand"/>.</summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Email is required.");
    }
}
