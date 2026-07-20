using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Identity.VerifyEmail;

/// <summary>
/// Handles <see cref="VerifyEmailCommand"/>: looks up the token by hash, and if redeemable, consumes
/// it and verifies the user's email (Auth §5). The operation is idempotent for an already-verified
/// account. An unusable token returns a single generic error.
/// </summary>
public sealed partial class VerifyEmailCommandHandler(
    IEmailVerificationTokenRepository tokens,
    IUserRepository users,
    ISecureTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<VerifyEmailCommandHandler> logger)
    : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenGenerator.Hash(request.Token);

        var token = await tokens.GetByTokenHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);
        if (token is null || !token.IsRedeemableAt(now))
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        var user = await users.GetByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        var verifyResult = user.VerifyEmail(now);
        if (verifyResult.IsFailure)
        {
            return verifyResult;
        }

        token.Consume(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        EmailVerified(logger, user.Id.Value);
        return Result.Success();
    }

    [LoggerMessage(
        EventId = 2300,
        Level = LogLevel.Information,
        Message = "Email verified for user {UserId}.")]
    private static partial void EmailVerified(ILogger logger, Guid userId);
}
