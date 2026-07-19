using IELTSLms.Abstractions.Email;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Infrastructure.Email;

/// <summary>
/// Placeholder email transport for Sprint 2: logs that a message would be sent instead of calling an
/// external provider (Auth §5 — SMTP/SendGrid deferred). Subject and recipient are logged for
/// traceability; the body is not, so verification/reset links never land in the logs. A real
/// transport adapter replaces this without touching the application flows.
/// </summary>
public sealed partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        EmailDispatched(logger, message.Subject, message.To);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Information,
        Message = "Email '{Subject}' dispatched to {Recipient} (no transport configured; body omitted).")]
    private static partial void EmailDispatched(ILogger logger, string subject, string recipient);
}
