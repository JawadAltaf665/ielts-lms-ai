using IELTSLms.Abstractions.Email;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeEmailTemplateRenderer : IEmailTemplateRenderer
{
    public EmailMessage RenderEmailVerification(string toEmail, string verificationLink) =>
        new(toEmail, "Verify your email", verificationLink, verificationLink);

    public EmailMessage RenderPasswordReset(string toEmail, string resetLink) =>
        new(toEmail, "Reset your password", resetLink, resetLink);
}
