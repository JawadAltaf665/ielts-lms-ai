using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.ResendVerification;

/// <summary>
/// Re-sends the email-verification link for an account that has registered but not yet verified
/// (Auth §5). Like registration, the response is uniform whether or not the address exists or is
/// already verified, so the endpoint cannot be used to enumerate accounts.
/// </summary>
/// <param name="Email">The email to re-send verification for.</param>
public sealed record ResendVerificationCommand(string Email) : ICommand;
