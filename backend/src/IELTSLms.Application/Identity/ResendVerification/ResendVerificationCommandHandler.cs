using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Identity.ResendVerification;

/// <summary>
/// Handles <see cref="ResendVerificationCommand"/>: if the email maps to an unverified account,
/// issues a fresh hashed verification token and re-sends the link (Auth §5). Returns success
/// uniformly for unknown or already-verified accounts so the endpoint cannot be used to enumerate
/// accounts or their verification state.
/// </summary>
public sealed partial class ResendVerificationCommandHandler(
    IUserRepository users,
    IEmailVerificationTokenRepository verificationTokens,
    ISecureTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    IOptions<AuthOptions> options,
    ILogger<ResendVerificationCommandHandler> logger)
    : ICommandHandler<ResendVerificationCommand>
{
    private readonly AuthOptions _options = options.Value;

    public async Task<Result> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            // Don't reveal that the address was malformed vs. simply not registered.
            return Result.Success();
        }

        var user = await users.GetByEmailAsync(emailResult.Value.Value, cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsDeleted || user.IsEmailVerified)
        {
            ResendForIneligibleAccount(logger);
            return Result.Success();
        }

        var secureToken = tokenGenerator.Generate();
        var verificationToken = EmailVerificationToken.Issue(
            EmailVerificationTokenId.From(guids.NewGuid()),
            user.Id,
            secureToken.Hash,
            now,
            _options.EmailVerificationTokenLifetime);
        verificationTokens.Add(verificationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var link = BuildLink(_options.EmailVerificationUrl, secureToken.RawValue);
        var message = templateRenderer.RenderEmailVerification(user.Email.Value, link);
        await emailSender.SendAsync(message, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static string BuildLink(string baseUrl, string token)
    {
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}token={Uri.EscapeDataString(token)}";
    }

    [LoggerMessage(
        EventId = 2500,
        Level = LogLevel.Information,
        Message = "Verification resend requested for an unknown or already-verified email; responding uniformly.")]
    private static partial void ResendForIneligibleAccount(ILogger logger);
}
