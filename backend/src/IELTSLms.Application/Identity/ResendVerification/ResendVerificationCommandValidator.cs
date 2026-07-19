using FluentValidation;

namespace IELTSLms.Application.Identity.ResendVerification;

/// <summary>
/// Presence-only validation for <see cref="ResendVerificationCommand"/>. Eligibility (unknown vs.
/// already-verified) is resolved by the handler and never surfaced, so validation must not reveal
/// anything about the account (Auth §5, enumeration resistance).
/// </summary>
public sealed class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Email is required.");
    }
}
