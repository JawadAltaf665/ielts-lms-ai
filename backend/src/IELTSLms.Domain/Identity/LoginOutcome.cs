namespace IELTSLms.Domain.Identity;

/// <summary>Outcome recorded for an authentication attempt in <see cref="LoginHistory"/>.</summary>
public enum LoginOutcome
{
    /// <summary>Credentials verified and a session was issued.</summary>
    Succeeded = 0,

    /// <summary>Credentials did not match.</summary>
    InvalidCredentials = 1,

    /// <summary>Account was locked at the time of the attempt.</summary>
    Locked = 2,

    /// <summary>Account exists but email is not yet verified.</summary>
    NotVerified = 3,

    /// <summary>Account is suspended or otherwise not permitted to authenticate.</summary>
    NotAllowed = 4,
}
