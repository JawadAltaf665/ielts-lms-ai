namespace IELTSLms.Abstractions.Security;

/// <summary>
/// Validates candidate passwords against the configured strength policy (Auth §5: minimum length,
/// complexity/passphrase support). Kept as a port so the rules are configuration-driven and the
/// application/validation layers do not hard-code them.
/// </summary>
public interface IPasswordPolicy
{
    /// <summary>The minimum acceptable password length, surfaced for validator messages.</summary>
    int MinimumLength { get; }

    /// <summary>Checks a candidate password, returning the failures (empty when it satisfies the policy).</summary>
    IReadOnlyList<PasswordPolicyViolation> Validate(string password);
}

/// <summary>A single password-policy failure.</summary>
/// <param name="Code">Stable machine code for the rule that failed (e.g. <c>PASSWORD_TOO_SHORT</c>).</param>
/// <param name="Message">Human-readable description of the failed rule.</param>
public readonly record struct PasswordPolicyViolation(string Code, string Message);
