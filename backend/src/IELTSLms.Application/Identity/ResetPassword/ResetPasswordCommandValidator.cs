using FluentValidation;
using IELTSLms.Abstractions.Security;

namespace IELTSLms.Application.Identity.ResetPassword;

/// <summary>
/// Validates <see cref="ResetPasswordCommand"/>: the token must be present and the new password must
/// satisfy the configured <see cref="IPasswordPolicy"/> (Auth §5).
/// </summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IPasswordPolicy passwordPolicy)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("A reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("A new password is required.")
            .Custom((password, context) =>
            {
                if (string.IsNullOrEmpty(password))
                {
                    return;
                }

                foreach (var violation in passwordPolicy.Validate(password))
                {
                    context.AddFailure(new FluentValidation.Results.ValidationFailure(
                        nameof(ResetPasswordCommand.NewPassword), violation.Message)
                    {
                        ErrorCode = violation.Code,
                    });
                }
            });
    }
}
