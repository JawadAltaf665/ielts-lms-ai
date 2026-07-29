using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>
/// Dead-letter ledger access for <see cref="FailedWebhookEvent"/> (ERD ENT-039T). Not an
/// <see cref="IRepository{TAggregate,TId}"/> — the entity is not an aggregate root.
/// </summary>
public interface IFailedWebhookEventRepository
{
    Task<FailedWebhookEvent?> GetByIdAsync(string stripeEventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FailedWebhookEvent>> ListRetryableAsync(CancellationToken cancellationToken = default);

    void Add(FailedWebhookEvent failedWebhookEvent);

    void Remove(FailedWebhookEvent failedWebhookEvent);
}
