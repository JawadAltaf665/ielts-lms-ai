using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="SubscriptionItem"/> (ERD ENT-039N).</summary>
public interface ISubscriptionItemRepository : IRepository<SubscriptionItem, SubscriptionItemId>
{
    Task<IReadOnlyList<SubscriptionItem>> ListBySubscriptionAsync(SubscriptionId subscriptionId, CancellationToken cancellationToken = default);
}
