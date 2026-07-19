using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.ResetPassword;

/// <summary>
/// Completes a password reset: validates the token, sets the new password, and revokes all existing
/// sessions so any stolen credential is invalidated (Auth §4.3, §5).
/// </summary>
/// <param name="Token">The opaque reset token from the emailed link.</param>
/// <param name="NewPassword">The new plaintext password (validated against policy).</param>
public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand;
