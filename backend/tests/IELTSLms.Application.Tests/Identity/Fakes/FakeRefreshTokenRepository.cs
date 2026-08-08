using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly Dictionary<RefreshTokenId, RefreshToken> _byId = [];

    public IReadOnlyCollection<RefreshToken> Added => _byId.Values;

    public Task<RefreshToken?> GetByIdAsync(RefreshTokenId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(RefreshToken aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(RefreshToken aggregate) => _byId.Remove(aggregate.Id);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetBySessionAsync(UserSessionId sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(_byId.Values.Where(t => t.SessionId == sessionId).ToList());
}
