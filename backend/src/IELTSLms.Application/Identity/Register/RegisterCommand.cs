using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Register;

/// <summary>
/// Registers a new account and sends an email-verification link (Auth §5). The response is
/// deliberately uniform whether or not the email was already taken, so the endpoint does not leak
/// which addresses are registered (enumeration resistance).
/// </summary>
/// <param name="Email">The email to register.</param>
/// <param name="Password">The plaintext password (validated against policy, never stored).</param>
/// <param name="DisplayName">The user's display name.</param>
public sealed record RegisterCommand(string Email, string Password, string DisplayName) : ICommand;
