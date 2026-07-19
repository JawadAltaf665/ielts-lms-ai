using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Identity.ResetPassword;

/// <summary>
/// Handles <see cref="ResetPasswordCommand"/>: validates the reset token, sets the new password
/// hash, consumes the token, and revokes every active session and its refresh-token family so any
/// credential the attacker may hold is invalidated (Auth §4.3, §5).
/// </summary>
public sealed partial class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository resetTokens,
    IUserRepository users,
    IUserSessionRepository sessions,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ResetPasswordCommandHandler> logger)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenGenerator.Hash(request.Token);

        var token = await resetTokens.GetByTokenHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);
        if (token is null || !token.IsRedeemableAt(now))
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        var user = await users.GetByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsDeleted)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        user.ChangePassword(PasswordHash.FromHashed(passwordHasher.Hash(request.NewPassword)), now);
        token.Consume(now);

        // Invalidate every session and refresh token so a reset always logs other devices out.
        const string reason = "Password was reset.";
        var activeSessions = await sessions.GetActiveByUserAsync(user.Id, now, cancellationToken).ConfigureAwait(false);
        foreach (var session in activeSessions)
        {
            session.Revoke(reason, now);
            var family = await refreshTokens.GetBySessionAsync(session.Id, cancellationToken).ConfigureAwait(false);
            foreach (var refreshToken in family)
            {
                refreshToken.Revoke(reason, now);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        PasswordReset(logger, user.Id.Value, activeSessions.Count);
        return Result.Success();
    }

    [LoggerMessage(
        EventId = 2400,
        Level = LogLevel.Information,
        Message = "Password reset for user {UserId}; {SessionCount} sessions revoked.")]
    private static partial void PasswordReset(ILogger logger, Guid userId, int sessionCount);
}
