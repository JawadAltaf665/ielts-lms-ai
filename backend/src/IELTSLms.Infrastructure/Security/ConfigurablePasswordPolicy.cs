using IELTSLms.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Configuration-driven password strength policy (Auth §5): a minimum length (≥12 per Security §6)
/// plus optional character-class requirements. Rules live here rather than in validators so they
/// are declared once and can change through configuration without code edits.
/// </summary>
public sealed class ConfigurablePasswordPolicy(IOptions<PasswordPolicyOptions> options) : IPasswordPolicy
{
    private readonly PasswordPolicyOptions _options = options.Value;

    /// <inheritdoc />
    public int MinimumLength => _options.MinimumLength;

    /// <inheritdoc />
    public IReadOnlyList<PasswordPolicyViolation> Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return [new PasswordPolicyViolation("PASSWORD_REQUIRED", "Password is required.")];
        }

        var violations = new List<PasswordPolicyViolation>();

        if (password.Length < _options.MinimumLength)
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_TOO_SHORT",
                $"Password must be at least {_options.MinimumLength} characters long."));
        }

        if (password.Length > _options.MaximumLength)
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_TOO_LONG",
                $"Password must not exceed {_options.MaximumLength} characters."));
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_MISSING_UPPERCASE", "Password must contain an uppercase letter."));
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_MISSING_LOWERCASE", "Password must contain a lowercase letter."));
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_MISSING_DIGIT", "Password must contain a digit."));
        }

        if (_options.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            violations.Add(new PasswordPolicyViolation(
                "PASSWORD_MISSING_SYMBOL", "Password must contain a non-alphanumeric character."));
        }

        return violations;
    }
}
