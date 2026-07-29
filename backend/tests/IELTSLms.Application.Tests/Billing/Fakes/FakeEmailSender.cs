using IELTSLms.Abstractions.Email;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeEmailSender : IEmailSender
{
    public List<EmailMessage> SentMessages { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        SentMessages.Add(message);
        return Task.CompletedTask;
    }
}
