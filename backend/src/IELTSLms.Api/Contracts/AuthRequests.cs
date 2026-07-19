namespace IELTSLms.Api.Contracts;

/// <summary>Request body for <c>POST /auth/register</c> (Auth §5).</summary>
/// <param name="Email">The email to register.</param>
/// <param name="Password">The plaintext password (validated against policy, never stored).</param>
/// <param name="DisplayName">The user's display name.</param>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);

/// <summary>Request body for <c>POST /auth/login</c>.</summary>
/// <param name="Email">The account email.</param>
/// <param name="Password">The plaintext password.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Request body for <c>POST /auth/refresh</c>. The refresh token may also be supplied via the
/// HttpOnly cookie; the body field is optional to support non-browser clients.
/// </summary>
/// <param name="RefreshToken">The opaque refresh token, if not sent as a cookie.</param>
public sealed record RefreshRequest(string? RefreshToken);

/// <summary>Request body for <c>POST /auth/forgot-password</c>.</summary>
/// <param name="Email">The account email to send a reset link to.</param>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Request body for <c>POST /auth/reset-password</c>.</summary>
/// <param name="Token">The opaque reset token from the emailed link.</param>
/// <param name="NewPassword">The new plaintext password (validated against policy).</param>
public sealed record ResetPasswordRequest(string Token, string NewPassword);

/// <summary>Request body for <c>POST /auth/verify-email</c>.</summary>
/// <param name="Token">The opaque verification token from the emailed link.</param>
public sealed record VerifyEmailRequest(string Token);

/// <summary>Request body for <c>POST /auth/resend-verification</c>.</summary>
/// <param name="Email">The account email to resend the verification link to.</param>
public sealed record ResendVerificationRequest(string Email);

/// <summary>
/// Response body for login/refresh. The opaque refresh token is never returned in the body — it is
/// delivered as an HttpOnly, Secure cookie so client-side script cannot read it (Auth §3.3). The
/// client stores the access token in memory only.
/// </summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="TokenType">The token type; always <c>Bearer</c>.</param>
/// <param name="AccessTokenExpiresAtUtc">When the access token expires (UTC), for scheduling refresh.</param>
/// <param name="SessionId">The device session id (the token's <c>sid</c> claim).</param>
public sealed record AccessTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresAtUtc,
    Guid SessionId);
