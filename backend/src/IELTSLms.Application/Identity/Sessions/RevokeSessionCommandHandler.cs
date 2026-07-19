using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity.Sessions;

/// <summary>
/// Handles <see cref="RevokeSessionCommand"/>: revokes the named session and its refresh-token
/// family when it belongs to the caller. Idempotent — an unknown session, another user's session,
/// or an already-revoked one still returns success so no session state leaks.
/// </summary>
public sealed class RevokeSessionCommandHandler(
    IUserSessionRepository sessions,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IClock clock)
    : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var sessionId = UserSessionId.From(request.SessionId);

        var session = await sessions.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.UserId.Value != request.UserId)
        {
            return Result.Success();
        }

        const string reason = "Session revoked by user.";
        session.Revoke(reason, now);

        var family = await refreshTokens.GetBySessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
        foreach (var token in family)
        {
            token.Revoke(reason, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
