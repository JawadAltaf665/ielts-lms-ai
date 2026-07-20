using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity;

/// <summary>
/// Central catalogue of stable auth error codes (Auth §10, API Design Standards §9.2). Codes are
/// part of the contract and must not change meaning once published. Several flows deliberately
/// return the same generic code to stay enumeration-resistant (Auth §5, §6).
/// </summary>
public static class AuthErrors
{
    /// <summary>Login failed. Deliberately generic so it cannot distinguish "no such user" from "wrong password".</summary>
    public static Error InvalidCredentials => Error.Unauthorized(
        "AUTH_INVALID_CREDENTIALS", "The email or password is incorrect.");

    /// <summary>A refresh/verification/reset token was missing, unknown, expired, or already used.</summary>
    public static Error InvalidToken => Error.Unauthorized(
        "AUTH_INVALID_TOKEN", "The token is invalid or has expired.");

    /// <summary>The account is temporarily locked after repeated failed logins.</summary>
    public static Error AccountLocked => Error.Forbidden(
        "AUTH_ACCOUNT_LOCKED", "This account is temporarily locked. Try again later.");

    /// <summary>No tenant was resolved for a request that operates within a tenant (Multi-Tenant Architecture §6.2).</summary>
    public static Error TenantRequired => Error.Unauthorized(
        "AUTH_TENANT_REQUIRED", "A tenant could not be determined for this request.");
}
