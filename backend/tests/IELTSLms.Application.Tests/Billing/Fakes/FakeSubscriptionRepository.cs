using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeSubscriptionRepository : ISubscriptionRepository
{
    private readonly List<Subscription> _subscriptions = [];

    public void Seed(Subscription subscription) => _subscriptions.Add(subscription);

    public Task<Subscription?> GetByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscriptions.FirstOrDefault(s => s.Id == id));

    public void Add(Subscription aggregate) => _subscriptions.Add(aggregate);

    public void Remove(Subscription aggregate) => _subscriptions.Remove(aggregate);

    public Task<Subscription?> GetCurrentByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscriptions
            .Where(s => s.TenantId == tenantId && s.Status != SubscriptionStatus.Expired)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefault());

    public Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscriptions.FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId));

    public Task<IReadOnlyList<Subscription>> ListTrialsEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Subscription>>(_subscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAtUtc < before).ToArray());

    public Task<IReadOnlyList<Subscription>> ListPastDueBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Subscription>>(_subscriptions
            .Where(s => s.Status == SubscriptionStatus.PastDue && s.UpdatedAtUtc < before).ToArray());

    public Task<IReadOnlyList<Subscription>> ListExpirableBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Subscription>>(_subscriptions
            .Where(s => s.Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended && s.CurrentPeriodEnd < before)
            .ToArray());

    public Task<IReadOnlyList<Subscription>> ListLinkedToProviderAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Subscription>>(_subscriptions.Where(s => s.StripeSubscriptionId is not null).ToArray());
}
