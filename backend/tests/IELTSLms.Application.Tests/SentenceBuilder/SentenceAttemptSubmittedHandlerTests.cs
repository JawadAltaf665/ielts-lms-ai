using IELTSLms.Abstractions.Events;
using IELTSLms.Application.SentenceBuilder.EventHandlers;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.SentenceBuilder.Events;

namespace IELTSLms.Application.Tests.SentenceBuilder;

/// <summary>Contract test: firing <c>SentenceAttemptSubmitted</c> enqueues the AI scaffolding job when
/// entitled and within quota, and silently skips (no failure) otherwise — "AI is additive, not gating".</summary>
public sealed class SentenceAttemptSubmittedHandlerTests
{
    private static DomainEventNotification<SentenceAttemptSubmittedDomainEvent> Notification() =>
        new(new SentenceAttemptSubmittedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "text"));

    [Fact]
    public async Task Entitled_and_within_quota_enqueues_the_job()
    {
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = new SentenceAttemptSubmittedHandler(
            new FakeEntitlementService(), new FakeQuotaService(), backgroundJobs);

        await handler.Handle(Notification(), CancellationToken.None);

        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Not_entitled_silently_skips_without_touching_quota_or_enqueueing()
    {
        var quotas = new FakeQuotaService();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = new SentenceAttemptSubmittedHandler(
            new FakeEntitlementService { HasFeature = false }, quotas, backgroundJobs);

        await handler.Handle(Notification(), CancellationToken.None);

        Assert.Equal(0, quotas.ConsumeCallCount);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Quota_exceeded_silently_skips_without_enqueueing()
    {
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = new SentenceAttemptSubmittedHandler(
            new FakeEntitlementService(), new FakeQuotaService { Allowed = false }, backgroundJobs);

        await handler.Handle(Notification(), CancellationToken.None);

        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }
}
