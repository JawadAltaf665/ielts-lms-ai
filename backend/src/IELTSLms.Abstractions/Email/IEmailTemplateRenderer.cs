namespace IELTSLms.Abstractions.Email;

/// <summary>
/// Renders the transactional email bodies for identity flows into an <see cref="EmailMessage"/>.
/// Kept behind a port so templating technology and content can change without touching the
/// application flows (Auth §5: email templates abstraction).
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>Builds the "verify your email" message containing the verification link.</summary>
    EmailMessage RenderEmailVerification(string toEmail, string verificationLink);

    /// <summary>Builds the "reset your password" message containing the reset link.</summary>
    EmailMessage RenderPasswordReset(string toEmail, string resetLink);
}
