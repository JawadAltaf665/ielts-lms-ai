using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Identity.Login;

/// <summary>
/// Handles <see cref="LoginCommand"/>: verifies credentials, applies lockout, and on success opens a
/// session and issues a token pair (Auth §3, §4, §6). Failures return a single generic error and the
/// handler performs a dummy hash for unknown accounts, so response content and timing do not reveal
/// whether an email exists (enumeration resistance). Every attempt is written to login history.
/// </summary>
public sealed partial class LoginCommandHandler(
    IUserRepository users,
    IUserSessionRepository sessions,
    IRefreshTokenRepository refreshTokens,
    ILoginHistoryRepository loginHistory,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator tokenGenerator,
    IAccessTokenService accessTokenService,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    IOptions<AuthOptions> options,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, AuthTokensResult>
{
    private readonly AuthOptions _options = options.Value;

    public async Task<Result<AuthTokensResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        // Login operates within a tenant: the account lookup is tenant-scoped and every attempt is
        // recorded against the tenant. Without one resolved (X-Tenant-Id pre-auth) we can't proceed.
        if (tenantContext.TenantId is not { } tenantId)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.TenantRequired);
        }

        var normalizedEmail = Email.Create(request.Email) is { IsSuccess: true } e ? e.Value.Value : request.Email.Trim().ToLowerInvariant();

        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            // Spend comparable time hashing so a missing account is indistinguishable by timing.
            passwordHasher.Verify(request.Password, DummyHash);
            await RecordAttemptAsync(tenantId, null, normalizedEmail, LoginOutcome.InvalidCredentials, request, now, cancellationToken)
                .ConfigureAwait(false);
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidCredentials);
        }

        var canAuthenticate = user.EnsureCanAuthenticate(now);
        if (canAuthenticate.IsFailure)
        {
            var outcome = MapOutcome(canAuthenticate.Error.Code);
            await RecordAttemptAsync(tenantId, user.Id, normalizedEmail, outcome, request, now, cancellationToken).ConfigureAwait(false);

            // Locked accounts get the explicit lockout error; other non-credential blocks stay generic.
            return canAuthenticate.Error.Code == "AUTH_ACCOUNT_LOCKED"
                ? Result.Failure<AuthTokensResult>(AuthErrors.AccountLocked)
                : Result.Failure<AuthTokensResult>(AuthErrors.InvalidCredentials);
        }

        var verification = passwordHasher.Verify(request.Password, user.PasswordHash.Value);
        if (!verification.IsMatch)
        {
            user.RegisterFailedLogin(_options.MaxFailedLoginAttempts, _options.LockoutDuration, now);
            await RecordAttemptAsync(tenantId, user.Id, normalizedEmail, LoginOutcome.InvalidCredentials, request, now, cancellationToken)
                .ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidCredentials);
        }

        // Upgrade the stored hash opportunistically if the hasher's parameters have since changed.
        if (verification.NeedsRehash)
        {
            user.ChangePassword(PasswordHash.FromHashed(passwordHasher.Hash(request.Password)), now);
        }

        var session = UserSession.Start(
            UserSessionId.From(guids.NewGuid()),
            user.TenantId,
            user.Id,
            request.UserAgent,
            request.IpAddress,
            request.UserAgent,
            now,
            _options.RefreshTokenSlidingLifetime,
            _options.SessionAbsoluteLifetime);
        sessions.Add(session);

        var refreshRaw = tokenGenerator.Generate();
        var refreshToken = RefreshToken.Issue(
            RefreshTokenId.From(guids.NewGuid()),
            user.TenantId,
            user.Id,
            session.Id,
            refreshRaw.Hash,
            now,
            _options.RefreshTokenSlidingLifetime);
        refreshTokens.Add(refreshToken);

        user.RegisterSuccessfulLogin(session.Id, now);
        await RecordAttemptAsync(tenantId, user.Id, normalizedEmail, LoginOutcome.Succeeded, request, now, cancellationToken)
            .ConfigureAwait(false);

        var accessToken = accessTokenService.CreateToken(new AccessTokenRequest(
            user.Id.Value,
            user.TenantId,
            session.Id.Value,
            user.Email.Value,
            [IdentityDefaults.PasswordAuthenticationMethod]));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        UserLoggedIn(logger, user.Id.Value, session.Id.Value);

        return Result.Success(new AuthTokensResult(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshRaw.RawValue,
            session.ExpiresAtUtc,
            session.Id.Value));
    }

    private Task RecordAttemptAsync(
        Guid tenantId,
        UserId? userId,
        string email,
        LoginOutcome outcome,
        LoginCommand request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        loginHistory.Add(LoginHistory.Record(
            LoginHistoryId.From(guids.NewGuid()),
            tenantId,
            userId,
            email,
            outcome,
            request.IpAddress,
            request.UserAgent,
            now));
        return Task.CompletedTask;
    }

    private static LoginOutcome MapOutcome(string errorCode) => errorCode switch
    {
        "AUTH_ACCOUNT_LOCKED" => LoginOutcome.Locked,
        "AUTH_EMAIL_NOT_VERIFIED" => LoginOutcome.NotVerified,
        "AUTH_ACCOUNT_DISABLED" => LoginOutcome.NotAllowed,
        _ => LoginOutcome.InvalidCredentials,
    };

    [LoggerMessage(
        EventId = 2200,
        Level = LogLevel.Information,
        Message = "User {UserId} logged in on session {SessionId}.")]
    private static partial void UserLoggedIn(ILogger logger, Guid userId, Guid sessionId);

    // A syntactically valid Argon2 hash of a random value, used only to equalize timing for unknown
    // accounts. It never matches a real password.
    private const string DummyHash =
        "$argon2id$v=19$m=19456,t=2,p=1$c29tZXNhbHRzb21lc2FsdA$RdescudvJCsgt3ub+b+dWRWJTmaaJObG";
}
