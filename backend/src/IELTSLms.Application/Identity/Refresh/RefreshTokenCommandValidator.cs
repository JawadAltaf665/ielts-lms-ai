using FluentValidation;

namespace IELTSLms.Application.Identity.Refresh;

/// <summary>Presence validation for <see cref="RefreshTokenCommand"/>.</summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("A refresh token is required.");
    }
}
