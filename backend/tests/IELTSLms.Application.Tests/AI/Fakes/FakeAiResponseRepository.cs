using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiResponseRepository : IAiResponseRepository
{
    private readonly List<AiResponse> _responses = [];
    private readonly Dictionary<AiResponseId, (string Capability, string Provider)> _usageMeta = [];

    public IReadOnlyList<AiResponse> Responses => _responses;

    public void Seed(AiResponse response) => _responses.Add(response);

    /// <summary>Seeds a response along with the capability/provider labels the real repository would
    /// derive via a join to <c>AiPrompt</c>/<c>AiProvider</c>, for <see cref="GetUsageSummaryAsync"/>.</summary>
    public void Seed(AiResponse response, string capability, string provider)
    {
        _responses.Add(response);
        _usageMeta[response.Id] = (capability, provider);
    }

    public Task<AiResponse?> GetByIdAsync(AiResponseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_responses.FirstOrDefault(r => r.Id == id));

    public void Add(AiResponse aggregate) => _responses.Add(aggregate);

    public void Remove(AiResponse aggregate) => _responses.Remove(aggregate);

    public Task<IReadOnlyList<AiUsageSummaryRow>> GetUsageSummaryAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        var rows = _responses
            .Where(r => r.TenantId == tenantId && r.CreatedAtUtc >= fromUtc && r.CreatedAtUtc < toUtc && _usageMeta.ContainsKey(r.Id))
            .GroupBy(r => (_usageMeta[r.Id].Capability, _usageMeta[r.Id].Provider, Day: DateOnly.FromDateTime(r.CreatedAtUtc.UtcDateTime), r.CostCurrency))
            .Select(g => new AiUsageSummaryRow(
                g.Key.Capability, g.Key.Provider, g.Key.Day, g.Count(), g.Sum(x => (long)x.PromptTokens),
                g.Sum(x => (long)x.CompletionTokens), g.Sum(x => x.CostMinorUnits), g.Key.CostCurrency))
            .ToList();

        return Task.FromResult<IReadOnlyList<AiUsageSummaryRow>>(rows);
    }
}
