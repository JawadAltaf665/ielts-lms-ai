using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Identity.ForgotPassword;

/// <summary>
/// Handles <see cref="ForgotPasswordCommand"/>: if the email maps to an eligible account, issues a
/// hashed, time-boxed reset token and emails the link (Auth §5). Returns success uniformly and does
/// no per-account work observable to the caller when the address is unknown, so accounts cannot be
/// enumerated.
/// </summary>
public sealed partial class ForgotPasswordCommandHandler(
    IUserRepository users,
    IPasswordResetTokenRepository resetTokens,
    ISecureTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    IOptions<AuthOptions> options,
    ILogger<ForgotPasswordCommandHandler> logger)
    : ICommandHandler<ForgotPasswordCommand>
{
    private readonly AuthOptions _options = options.Value;

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            // Don't reveal that the address was malformed vs. simply not registered.
            return Result.Success();
        }

        var user = await users.GetByEmailAsync(emailResult.Value.Value, cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsDeleted)
        {
            ResetRequestedForUnknownEmail(logger);
            return Result.Success();
        }

        var secureToken = tokenGenerator.Generate();
        var resetToken = PasswordResetToken.Issue(
            PasswordResetTokenId.From(guids.NewGuid()),
            user.TenantId,
            user.Id,
            secureToken.Hash,
            now,
            _options.PasswordResetTokenLifetime);
        resetTokens.Add(resetToken);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var link = BuildLink(_options.PasswordResetUrl, secureToken.RawValue);
        var message = templateRenderer.RenderPasswordReset(user.Email.Value, link);
        await emailSender.SendAsync(message, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static string BuildLink(string baseUrl, string token)
    {
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}token={Uri.EscapeDataString(token)}";
    }

    [LoggerMessage(
        EventId = 2200,
        Level = LogLevel.Information,
        Message = "Password-reset requested for an unknown email; responding uniformly.")]
    private static partial void ResetRequestedForUnknownEmail(ILogger logger);
}
