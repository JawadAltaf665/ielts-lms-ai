using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity.Logout;

/// <summary>
/// Handles <see cref="LogoutCommand"/>: revokes the named session and its refresh-token family. The
/// session must belong to the calling user; a mismatch or missing session is treated as an
/// already-completed logout and still returns success (idempotent, no state leak).
/// </summary>
public sealed class LogoutCommandHandler(
    IUserSessionRepository sessions,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IClock clock)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var sessionId = UserSessionId.From(request.SessionId);

        var session = await sessions.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.UserId.Value != request.UserId)
        {
            return Result.Success();
        }

        const string reason = "User logged out.";
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
