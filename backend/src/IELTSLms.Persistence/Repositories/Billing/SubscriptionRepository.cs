using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class SubscriptionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Subscription, SubscriptionId>(dbContext), ISubscriptionRepository
{
    public async Task<Subscription?> GetCurrentByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.Where(s => s.TenantId == tenantId && s.Status != SubscriptionStatus.Expired)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Subscription>> ListTrialsEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        await Set.Where(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAtUtc != null && s.TrialEndsAtUtc < before)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Subscription>> ListPastDueBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        await Set.Where(s => s.Status == SubscriptionStatus.PastDue && s.UpdatedAtUtc != null && s.UpdatedAtUtc < before)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Subscription>> ListExpirableBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        await Set.Where(s =>
                (s.Status == SubscriptionStatus.Cancelled || s.Status == SubscriptionStatus.Suspended)
                && s.CurrentPeriodEnd != null && s.CurrentPeriodEnd < before)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Subscription>> ListLinkedToProviderAsync(CancellationToken cancellationToken = default) =>
        await Set.Where(s =>
                s.StripeSubscriptionId != null
                && (s.Status == SubscriptionStatus.Trial || s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
}
