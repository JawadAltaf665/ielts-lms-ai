using System.Net;
using IELTSLms.Abstractions.Email;

namespace IELTSLms.Infrastructure.Email;

/// <summary>
/// Renders the identity flows' transactional emails from in-code templates (Auth §5: templates
/// abstraction). Kept deliberately simple — a richer templating engine can replace this behind the
/// same port without touching the flows. All caller-supplied values are HTML-encoded to prevent
/// injection into the HTML body.
/// </summary>
public sealed class IdentityEmailTemplateRenderer : IEmailTemplateRenderer
{
    /// <inheritdoc />
    public EmailMessage RenderEmailVerification(string toEmail, string verificationLink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationLink);

        var encodedLink = WebUtility.HtmlEncode(verificationLink);
        const string subject = "Verify your email address";

        var html =
            $"""
             <p>Welcome to IELTS LMS AI.</p>
             <p>Please confirm your email address by clicking the link below. This link expires shortly for your security.</p>
             <p><a href="{encodedLink}">Verify my email</a></p>
             <p>If you did not create this account, you can safely ignore this message.</p>
             """;

        var text =
            $"""
             Welcome to IELTS LMS AI.

             Please confirm your email address by opening the link below. This link expires shortly for your security.

             {verificationLink}

             If you did not create this account, you can safely ignore this message.
             """;

        return new EmailMessage(toEmail, subject, html, text);
    }

    /// <inheritdoc />
    public EmailMessage RenderPasswordReset(string toEmail, string resetLink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetLink);

        var encodedLink = WebUtility.HtmlEncode(resetLink);
        const string subject = "Reset your password";

        var html =
            $"""
             <p>We received a request to reset your IELTS LMS AI password.</p>
             <p>Click the link below to choose a new password. This link expires shortly and can be used only once.</p>
             <p><a href="{encodedLink}">Reset my password</a></p>
             <p>If you did not request this, you can safely ignore this message; your password will not change.</p>
             """;

        var text =
            $"""
             We received a request to reset your IELTS LMS AI password.

             Open the link below to choose a new password. This link expires shortly and can be used only once.

             {resetLink}

             If you did not request this, you can safely ignore this message; your password will not change.
             """;

        return new EmailMessage(toEmail, subject, html, text);
    }
}
