using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Identity.Register;

/// <summary>
/// Handles <see cref="RegisterCommand"/>: creates a pending account, issues a hashed
/// email-verification token, and emails the link. Returns success uniformly even when the email is
/// already registered so the endpoint cannot be used to enumerate accounts (Auth §5); in that case
/// no new account is created and no email is sent.
/// </summary>
public sealed partial class RegisterCommandHandler(
    IUserRepository users,
    IEmailVerificationTokenRepository verificationTokens,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    IOptions<AuthOptions> options,
    ILogger<RegisterCommandHandler> logger)
    : ICommandHandler<RegisterCommand>
{
    private readonly AuthOptions _options = options.Value;

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var email = emailResult.Value;
        var now = clock.UtcNow;

        // Enumeration resistance: if the address is taken, do the same "success" work the caller sees
        // without creating a second account or leaking that it exists (Auth §5).
        if (await users.ExistsByEmailAsync(email.Value, cancellationToken).ConfigureAwait(false))
        {
            RegistrationForExistingEmail(logger);
            return Result.Success();
        }

        var passwordHash = PasswordHash.FromHashed(passwordHasher.Hash(request.Password));
        var userId = UserId.From(guids.NewGuid());
        var user = User.Register(userId, IdentityDefaults.TenantId, email, passwordHash, request.DisplayName, now);
        users.Add(user);

        var secureToken = tokenGenerator.Generate();
        var verificationToken = EmailVerificationToken.Issue(
            EmailVerificationTokenId.From(guids.NewGuid()),
            userId,
            secureToken.Hash,
            now,
            _options.EmailVerificationTokenLifetime);
        verificationTokens.Add(verificationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var link = BuildLink(_options.EmailVerificationUrl, secureToken.RawValue);
        var message = templateRenderer.RenderEmailVerification(email.Value, link);
        await emailSender.SendAsync(message, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static string BuildLink(string baseUrl, string token)
    {
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}token={Uri.EscapeDataString(token)}";
    }

    [LoggerMessage(
        EventId = 2100,
        Level = LogLevel.Information,
        Message = "Registration attempted for an already-registered email; responding uniformly.")]
    private static partial void RegistrationForExistingEmail(ILogger logger);
}
