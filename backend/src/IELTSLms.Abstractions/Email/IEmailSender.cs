namespace IELTSLms.Abstractions.Email;

/// <summary>
/// Transport-agnostic email delivery port (Auth §5, email workflow). A concrete SMTP/SendGrid
/// adapter is deferred to a later sprint; the composition root binds a no-op/logging implementation
/// for now so the flows are exercised end-to-end without an external provider.
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends a single message. Implementations must not throw for transient delivery issues that the flow can tolerate.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

/// <summary>A ready-to-send email message.</summary>
/// <param name="To">Recipient address.</param>
/// <param name="Subject">Message subject.</param>
/// <param name="HtmlBody">Rendered HTML body.</param>
/// <param name="TextBody">Plain-text alternative body.</param>
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);
