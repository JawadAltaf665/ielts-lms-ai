using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakePlanRepository : IPlanRepository
{
    private readonly List<Plan> _plans = [];

    public void Seed(Plan plan) => _plans.Add(plan);

    public Task<Plan?> GetByIdAsync(PlanId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_plans.FirstOrDefault(p => p.Id == id));

    public void Add(Plan aggregate) => _plans.Add(aggregate);

    public void Remove(Plan aggregate) => _plans.Remove(aggregate);

    public Task<Plan?> GetByCodeAsync(PlanCode code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_plans.FirstOrDefault(p => p.Code == code));

    public Task<IReadOnlyList<Plan>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Plan>>(_plans.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToArray());
}

internal sealed class FakePriceRepository : IPriceRepository
{
    private readonly List<Price> _prices = [];

    public void Seed(Price price) => _prices.Add(price);

    public Task<Price?> GetByIdAsync(PriceId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_prices.FirstOrDefault(p => p.Id == id));

    public void Add(Price aggregate) => _prices.Add(aggregate);

    public void Remove(Price aggregate) => _prices.Remove(aggregate);

    public Task<IReadOnlyList<Price>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Price>>(_prices.Where(p => p.PlanId == planId).ToArray());

    public Task<IReadOnlyList<Price>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Price>>(_prices.Where(p => p.IsActive).ToArray());
}
