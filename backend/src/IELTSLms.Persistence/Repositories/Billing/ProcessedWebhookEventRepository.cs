using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class ProcessedWebhookEventRepository(ApplicationDbContext dbContext) : IProcessedWebhookEventRepository
{
    private DbSet<ProcessedWebhookEvent> Set => dbContext.Set<ProcessedWebhookEvent>();

    public async Task<bool> ExistsAsync(string stripeEventId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(e => e.Id == stripeEventId, cancellationToken).ConfigureAwait(false);

    public void Add(ProcessedWebhookEvent processedWebhookEvent) => Set.Add(processedWebhookEvent);
}
