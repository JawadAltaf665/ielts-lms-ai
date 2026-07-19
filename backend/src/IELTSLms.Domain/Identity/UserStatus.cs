namespace IELTSLms.Domain.Identity;

/// <summary>
/// Lifecycle state of a <see cref="User"/> account. Stored as a stable string code, never an
/// integer, so the contract survives reordering (API Design Standards §7.2). Access control and
/// tenant concerns are layered on later; this is purely the account's own lifecycle.
/// </summary>
public enum UserStatus
{
    /// <summary>Registered but email not yet verified; cannot fully authenticate (Auth §5).</summary>
    PendingVerification = 0,

    /// <summary>Verified and able to authenticate.</summary>
    Active = 1,

    /// <summary>Temporarily blocked after repeated failed logins (Auth §6); auto-unlocks.</summary>
    Locked = 2,

    /// <summary>Administratively disabled; cannot authenticate until re-enabled.</summary>
    Suspended = 3,
}
