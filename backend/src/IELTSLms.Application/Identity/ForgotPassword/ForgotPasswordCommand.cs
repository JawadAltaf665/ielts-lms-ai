using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.ForgotPassword;

/// <summary>
/// Starts the password-reset flow by emailing a reset link (Auth §5). The response is uniform
/// whether or not the address is registered, so it cannot be used to enumerate accounts.
/// </summary>
/// <param name="Email">The account email to send a reset link to.</param>
public sealed record ForgotPasswordCommand(string Email) : ICommand;
