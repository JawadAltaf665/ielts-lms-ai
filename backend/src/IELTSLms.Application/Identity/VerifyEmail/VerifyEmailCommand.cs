using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.VerifyEmail;

/// <summary>
/// Consumes an email-verification token to activate the account (Auth §5). The token is single-use
/// and time-boxed; an unknown, expired, or already-used token fails generically.
/// </summary>
/// <param name="Token">The opaque verification token from the emailed link.</param>
public sealed record VerifyEmailCommand(string Token) : ICommand;
