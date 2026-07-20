using FluentValidation;
using IELTSLms.Abstractions.Security;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Identity.Register;

/// <summary>
/// Structural validation for <see cref="RegisterCommand"/>. Password strength is enforced through
/// the configured <see cref="IPasswordPolicy"/> so the rules stay in one place (Auth §5).
/// </summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IPasswordPolicy passwordPolicy)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Email is required.")
            .MaximumLength(Email.MaxLength).WithErrorCode("VALIDATION_EMAIL_TOO_LONG")
            .Must(email => Email.Create(email).IsSuccess)
            .WithErrorCode("VALIDATION_EMAIL_INVALID").WithMessage("Email is not a valid address.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Display name is required.")
            .MaximumLength(256).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Password is required.")
            .Custom((password, context) =>
            {
                if (string.IsNullOrEmpty(password))
                {
                    return;
                }

                foreach (var violation in passwordPolicy.Validate(password))
                {
                    context.AddFailure(new FluentValidation.Results.ValidationFailure(
                        nameof(RegisterCommand.Password), violation.Message)
                    {
                        ErrorCode = violation.Code,
                    });
                }
            });
    }
}
