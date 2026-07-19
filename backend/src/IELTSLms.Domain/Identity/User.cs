using IELTSLms.Domain.Identity.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// The identity aggregate root: an account that can authenticate. It owns its credential, lifecycle
/// state, and lockout counters, and enforces its invariants through methods rather than open
/// setters (Development Standards §15, DDD). Tokens and sessions are separate aggregates referenced
/// by <see cref="UserId"/> to keep this aggregate's consistency boundary small (Backend
/// Architecture §9).
/// </summary>
public sealed class User : AggregateRoot<UserId>, IAuditableEntity, ISoftDeletable
{
    private User(
        UserId id,
        Guid tenantId,
        Email email,
        PasswordHash passwordHash,
        string displayName,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        Status = UserStatus.PendingVerification;
        CreatedAtUtc = createdAtUtc;
    }

    private User()
    {
        // EF Core materialization.
        Email = null!;
        PasswordHash = null!;
        DisplayName = null!;
    }

    /// <summary>
    /// Owning tenant. Tenancy is not enforced until Sprint 3; the column exists now so the schema is
    /// stable and rows are attributable (Auth §12 groundwork).
    /// </summary>
    public Guid TenantId { get; private init; }

    /// <summary>The account's unique, normalized email address.</summary>
    public Email Email { get; private set; }

    /// <summary>The current password credential (hashed).</summary>
    public PasswordHash PasswordHash { get; private set; }

    /// <summary>Display name shown in the UI.</summary>
    public string DisplayName { get; private set; }

    /// <summary>Account lifecycle state.</summary>
    public UserStatus Status { get; private set; }

    /// <summary>When the email was verified (UTC), or <see langword="null"/> if not yet verified.</summary>
    public DateTimeOffset? EmailVerifiedAtUtc { get; private set; }

    /// <summary><see langword="true"/> once the account's email has been verified.</summary>
    public bool IsEmailVerified => EmailVerifiedAtUtc is not null;

    /// <summary>Consecutive failed login attempts since the last success; drives lockout (Auth §6).</summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>When the current lockout ends (UTC), or <see langword="null"/> if not locked.</summary>
    public DateTimeOffset? LockoutEndsAtUtc { get; private set; }

    /// <summary>When the user last authenticated successfully (UTC).</summary>
    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin) maintained by PostgreSQL (Backend Architecture §9).</summary>
    public uint Version { get; private set; }

    /// <summary>Whether the account is currently locked out at the given instant.</summary>
    public bool IsLockedOutAt(DateTimeOffset nowUtc) => LockoutEndsAtUtc is { } until && until > nowUtc;

    /// <summary>
    /// Registers a new account in the pending-verification state and raises
    /// <see cref="UserRegisteredDomainEvent"/>. The email is assumed unique — the application layer
    /// checks uniqueness before calling and the database enforces it.
    /// </summary>
    public static User Register(
        UserId id,
        Guid tenantId,
        Email email,
        PasswordHash passwordHash,
        string displayName,
        DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);

        var user = new User(id, tenantId, email, passwordHash, displayName.Trim(), nowUtc);
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(id.Value, tenantId, email.Value));
        return user;
    }

    /// <summary>
    /// Marks the email verified and activates a pending account. Idempotent for an already-verified
    /// account; returns a conflict only if the account is in a non-activatable state.
    /// </summary>
    public Result VerifyEmail(DateTimeOffset nowUtc)
    {
        if (IsEmailVerified)
        {
            return Result.Success();
        }

        if (Status is UserStatus.Suspended)
        {
            return Result.Failure(Error.Conflict("AUTH_ACCOUNT_NOT_ACTIVATABLE", "This account cannot be activated."));
        }

        EmailVerifiedAtUtc = nowUtc;
        Status = UserStatus.Active;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id.Value));
        return Result.Success();
    }

    /// <summary>
    /// Verifies the account can authenticate right now (verified, not locked, not suspended). The
    /// actual password comparison is performed by the infrastructure hasher; this guards the
    /// non-credential preconditions and keeps the reason for later auditing.
    /// </summary>
    public Result EnsureCanAuthenticate(DateTimeOffset nowUtc)
    {
        if (IsDeleted || Status is UserStatus.Suspended)
        {
            return Result.Failure(Error.Forbidden("AUTH_ACCOUNT_DISABLED", "This account is not permitted to sign in."));
        }

        if (IsLockedOutAt(nowUtc))
        {
            return Result.Failure(Error.Forbidden("AUTH_ACCOUNT_LOCKED", "This account is temporarily locked. Try again later."));
        }

        if (!IsEmailVerified)
        {
            return Result.Failure(Error.Forbidden("AUTH_EMAIL_NOT_VERIFIED", "Please verify your email address before signing in."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Records a successful authentication: clears failed-attempt counters, updates last-login, and
    /// raises <see cref="UserLoggedInDomainEvent"/> for the given session.
    /// </summary>
    public void RegisterSuccessfulLogin(UserSessionId sessionId, DateTimeOffset nowUtc)
    {
        FailedLoginAttempts = 0;
        LockoutEndsAtUtc = null;
        LastLoginAtUtc = nowUtc;

        if (Status is UserStatus.Locked)
        {
            Status = UserStatus.Active;
        }

        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new UserLoggedInDomainEvent(Id.Value, sessionId.Value));
    }

    /// <summary>
    /// Records a failed authentication and applies progressive lockout: once
    /// <paramref name="maxAttempts"/> consecutive failures accrue, the account is locked for
    /// <paramref name="lockoutDuration"/> and the counter resets (Auth §6).
    /// </summary>
    public void RegisterFailedLogin(int maxAttempts, TimeSpan lockoutDuration, DateTimeOffset nowUtc)
    {
        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be positive.");
        }

        FailedLoginAttempts++;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new UserLoginFailedDomainEvent(Id.Value, FailedLoginAttempts));

        if (FailedLoginAttempts >= maxAttempts)
        {
            var lockoutEnds = nowUtc.Add(lockoutDuration);
            LockoutEndsAtUtc = lockoutEnds;
            Status = UserStatus.Locked;
            FailedLoginAttempts = 0;
            RaiseDomainEvent(new UserLockedOutDomainEvent(Id.Value, lockoutEnds));
        }
    }

    /// <summary>
    /// Replaces the password credential (used by the reset flow) and raises
    /// <see cref="UserPasswordChangedDomainEvent"/> so existing sessions are revoked (Auth §5).
    /// Also clears any active lockout.
    /// </summary>
    public void ChangePassword(PasswordHash newPasswordHash, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(newPasswordHash);

        PasswordHash = newPasswordHash;
        FailedLoginAttempts = 0;
        LockoutEndsAtUtc = null;

        if (Status is UserStatus.Locked)
        {
            Status = UserStatus.Active;
        }

        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new UserPasswordChangedDomainEvent(Id.Value));
    }

    /// <summary>Soft-deletes the account (Development Standards §15); retained for audit/history.</summary>
    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
