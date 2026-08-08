using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeUserSessionRepository : IUserSessionRepository
{
    private readonly Dictionary<UserSessionId, UserSession> _byId = [];

    public IReadOnlyCollection<UserSession> Added => _byId.Values;

    public Task<UserSession?> GetByIdAsync(UserSessionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(UserSession aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(UserSession aggregate) => _byId.Remove(aggregate.Id);

    public Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(
        UserId userId, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserSession>>(
            _byId.Values.Where(s => s.UserId == userId && s.IsActiveAt(nowUtc)).ToList());
}
