using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Application.Identity;

/// <summary>
/// Configuration-driven auth timings and thresholds (Auth §4, §5, §6). Bound from configuration at
/// the composition root so lifetimes and lockout policy are not hard-coded in handlers.
/// </summary>
public sealed class AuthOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Auth";

    /// <summary>Access-token lifetime (~15 minutes per Auth §3.2).</summary>
    [Range(typeof(TimeSpan), "00:01:00", "01:00:00")]
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Sliding refresh/session window extended on each refresh (Auth §4.1).</summary>
    public TimeSpan RefreshTokenSlidingLifetime { get; set; } = TimeSpan.FromDays(14);

    /// <summary>Hard cap on a session's lifetime regardless of activity (Auth §4.1).</summary>
    public TimeSpan SessionAbsoluteLifetime { get; set; } = TimeSpan.FromDays(30);

    /// <summary>How long an email-verification token stays valid (Auth §5).</summary>
    public TimeSpan EmailVerificationTokenLifetime { get; set; } = TimeSpan.FromHours(24);

    /// <summary>How long a password-reset token stays valid (Auth §5).</summary>
    public TimeSpan PasswordResetTokenLifetime { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Consecutive failed logins that trip a lockout (Auth §6).</summary>
    [Range(1, 20)]
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>How long an account stays locked after tripping the threshold (Auth §6).</summary>
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Base URL of the email-verification page; the token is appended as a query string.</summary>
    [Required]
    public string EmailVerificationUrl { get; set; } = "https://localhost/verify-email";

    /// <summary>Base URL of the password-reset page; the token is appended as a query string.</summary>
    [Required]
    public string PasswordResetUrl { get; set; } = "https://localhost/reset-password";
}
