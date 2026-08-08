using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeLoginHistoryRepository : ILoginHistoryRepository
{
    private readonly Dictionary<LoginHistoryId, LoginHistory> _byId = [];

    public IReadOnlyCollection<LoginHistory> Added => _byId.Values;

    public Task<LoginHistory?> GetByIdAsync(LoginHistoryId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(LoginHistory aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(LoginHistory aggregate) => _byId.Remove(aggregate.Id);
}
