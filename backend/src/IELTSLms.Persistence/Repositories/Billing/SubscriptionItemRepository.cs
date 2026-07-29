using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class SubscriptionItemRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SubscriptionItem, SubscriptionItemId>(dbContext), ISubscriptionItemRepository
{
    public async Task<IReadOnlyList<SubscriptionItem>> ListBySubscriptionAsync(
        SubscriptionId subscriptionId, CancellationToken cancellationToken = default) =>
        await Set.Where(i => i.SubscriptionId == subscriptionId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
