using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Identity.Refresh;

/// <summary>
/// Handles <see cref="RefreshTokenCommand"/> with rotating, single-use refresh tokens and reuse
/// detection (Auth §4.2, §4.3):
/// <list type="bullet">
/// <item>An unknown, revoked, or expired token, or an inactive session, fails generically.</item>
/// <item>A token already consumed by rotation signals theft: the entire session family is revoked
/// and the session ended.</item>
/// <item>A valid token is marked rotated, chained to a freshly issued one, and the session slides.</item>
/// </list>
/// </summary>
public sealed partial class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserSessionRepository sessions,
    IUserRepository users,
    IUserRoleRepository userRoles,
    ISecureTokenGenerator tokenGenerator,
    IAccessTokenService accessTokenService,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    IOptions<AuthOptions> options,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, AuthTokensResult>
{
    private readonly AuthOptions _options = options.Value;

    public async Task<Result<AuthTokensResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenGenerator.Hash(request.RefreshToken);

        var presented = await refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);
        if (presented is null)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidToken);
        }

        // Reuse detection: a token that was already rotated must never be accepted again. Treat it as
        // a stolen-token replay and revoke the whole family + session (Auth §4.3).
        if (presented.IsUsed)
        {
            await RevokeFamilyAsync(presented.SessionId, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            RefreshTokenReuseDetected(logger, presented.SessionId.Value);
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidToken);
        }

        if (!presented.IsActiveAt(now))
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidToken);
        }

        var session = await sessions.GetByIdAsync(presented.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || !session.IsActiveAt(now))
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidToken);
        }

        var user = await users.GetByIdAsync(presented.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.EnsureCanAuthenticate(now).IsFailure)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidToken);
        }

        // Rotate: issue the replacement, then mark the presented token consumed and chain them.
        var newRaw = tokenGenerator.Generate();
        var replacement = RefreshToken.Issue(
            RefreshTokenId.From(guids.NewGuid()),
            user.TenantId,
            user.Id,
            session.Id,
            newRaw.Hash,
            now,
            _options.RefreshTokenSlidingLifetime);
        refreshTokens.Add(replacement);
        presented.MarkRotated(replacement.Id, now);

        session.SlideExpiry(now, _options.RefreshTokenSlidingLifetime, request.IpAddress, request.UserAgent);

        // Re-loaded on every refresh (not carried over from the presented token) so a role change
        // takes effect on the next refresh rather than lingering for the token's full lifetime.
        var roles = await userRoles.GetRoleNamesForUserAsync(user.Id.Value, user.TenantId, cancellationToken)
            .ConfigureAwait(false);

        var accessToken = accessTokenService.CreateToken(new AccessTokenRequest(
            user.Id.Value,
            user.TenantId,
            session.Id.Value,
            user.Email.Value,
            [IdentityDefaults.PasswordAuthenticationMethod],
            roles));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new AuthTokensResult(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            newRaw.RawValue,
            session.ExpiresAtUtc,
            session.Id.Value));
    }

    private async Task RevokeFamilyAsync(UserSessionId sessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        const string reason = "Refresh-token reuse detected.";

        var family = await refreshTokens.GetBySessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
        foreach (var token in family)
        {
            token.Revoke(reason, now);
        }

        var session = await sessions.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        session?.Revoke(reason, now);
    }

    [LoggerMessage(
        EventId = 2500,
        Level = LogLevel.Warning,
        Message = "Refresh-token reuse detected for session {SessionId}; family revoked.")]
    private static partial void RefreshTokenReuseDetected(ILogger logger, Guid sessionId);
}
