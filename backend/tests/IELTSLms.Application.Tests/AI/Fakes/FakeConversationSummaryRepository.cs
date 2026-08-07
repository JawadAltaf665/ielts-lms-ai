using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeConversationSummaryRepository : IConversationSummaryRepository
{
    private readonly List<ConversationSummary> _summaries = [];

    public void Seed(ConversationSummary summary) => _summaries.Add(summary);

    public Task<ConversationSummary?> GetByIdAsync(ConversationSummaryId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_summaries.FirstOrDefault(s => s.Id == id));

    public void Add(ConversationSummary aggregate) => _summaries.Add(aggregate);

    public void Remove(ConversationSummary aggregate) => _summaries.Remove(aggregate);

    public Task<ConversationSummary?> GetLatestAsync(Guid tenantId, ConversationId conversationId, CancellationToken cancellationToken = default)
    {
        var latest = _summaries
            .Where(s => s.TenantId == tenantId && s.ConversationId == conversationId)
            .OrderByDescending(s => s.CoversUpToSequence)
            .FirstOrDefault();
        return Task.FromResult(latest);
    }
}
