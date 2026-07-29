using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class FailedWebhookEventRepository(ApplicationDbContext dbContext) : IFailedWebhookEventRepository
{
    private DbSet<FailedWebhookEvent> Set => dbContext.Set<FailedWebhookEvent>();

    public async Task<FailedWebhookEvent?> GetByIdAsync(string stripeEventId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == stripeEventId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<FailedWebhookEvent>> ListRetryableAsync(CancellationToken cancellationToken = default) =>
        await Set.Where(e => e.RetryCount < FailedWebhookEvent.MaxRetryCount).ToListAsync(cancellationToken).ConfigureAwait(false);

    public void Add(FailedWebhookEvent failedWebhookEvent) => Set.Add(failedWebhookEvent);

    public void Remove(FailedWebhookEvent failedWebhookEvent) => Set.Remove(failedWebhookEvent);
}
