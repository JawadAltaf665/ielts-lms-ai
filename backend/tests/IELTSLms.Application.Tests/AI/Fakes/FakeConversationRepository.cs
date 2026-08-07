using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeConversationRepository : IConversationRepository
{
    private readonly List<Conversation> _conversations = [];

    public void Seed(Conversation conversation) => _conversations.Add(conversation);

    public Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_conversations.FirstOrDefault(c => c.Id == id));

    public void Add(Conversation aggregate) => _conversations.Add(aggregate);

    public void Remove(Conversation aggregate) => _conversations.Remove(aggregate);

    public Task<IReadOnlyList<Conversation>> ListByUserAsync(
        Guid tenantId, Guid userId, string? feature, ConversationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _conversations.Where(c => c.TenantId == tenantId && c.UserId == userId);
        if (feature is not null)
        {
            query = query.Where(c => c.Feature == feature);
        }

        if (status is not null)
        {
            query = query.Where(c => c.Status == status);
        }

        IReadOnlyList<Conversation> result = query
            .OrderByDescending(c => c.LastMessageAtUtc).ThenByDescending(c => c.Id.Value)
            .ToList();
        return Task.FromResult(result);
    }
}
