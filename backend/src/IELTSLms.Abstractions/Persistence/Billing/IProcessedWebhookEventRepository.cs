using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>
/// Idempotency ledger access for <see cref="ProcessedWebhookEvent"/> (ERD ENT-039P). Not an
/// <see cref="IRepository{TAggregate,TId}"/> — the entity is not an aggregate root.
/// </summary>
public interface IProcessedWebhookEventRepository
{
    Task<bool> ExistsAsync(string stripeEventId, CancellationToken cancellationToken = default);

    void Add(ProcessedWebhookEvent processedWebhookEvent);
}
