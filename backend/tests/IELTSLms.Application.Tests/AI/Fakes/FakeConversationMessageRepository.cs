using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeConversationMessageRepository : IConversationMessageRepository
{
    private readonly List<ConversationMessage> _messages = [];

    public void Seed(ConversationMessage message) => _messages.Add(message);

    public Task<ConversationMessage?> GetByIdAsync(ConversationMessageId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_messages.FirstOrDefault(m => m.Id == id));

    public void Add(ConversationMessage aggregate) => _messages.Add(aggregate);

    public void Remove(ConversationMessage aggregate) => _messages.Remove(aggregate);

    public Task<IReadOnlyList<ConversationMessage>> ListSinceSequenceAsync(
        Guid tenantId, ConversationId conversationId, int afterSequence, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ConversationMessage> result = _messages
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && m.Sequence > afterSequence)
            .OrderBy(m => m.Sequence)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<ConversationMessage>> ListPageAsync(
        Guid tenantId, ConversationId conversationId, int? beforeSequence, int limit, CancellationToken cancellationToken = default)
    {
        var query = _messages.Where(m => m.TenantId == tenantId && m.ConversationId == conversationId);
        if (beforeSequence is { } sequence)
        {
            query = query.Where(m => m.Sequence < sequence);
        }

        IReadOnlyList<ConversationMessage> result = query
            .OrderByDescending(m => m.Sequence)
            .Take(limit + 1)
            .ToList();
        return Task.FromResult(result);
    }
}
