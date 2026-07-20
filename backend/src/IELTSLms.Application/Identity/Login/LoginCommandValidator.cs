using FluentValidation;

namespace IELTSLms.Application.Identity.Login;

/// <summary>
/// Presence-only validation for <see cref="LoginCommand"/>. Credential correctness is checked by
/// the handler and always reported with a single generic error, so validation here must not reveal
/// anything about the account (Auth §6, enumeration resistance).
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Password is required.");
    }
}
