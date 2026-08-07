using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.Fakes;

internal sealed class FakeDailyPlanRepository : IDailyPlanRepository
{
    private readonly List<DailyPlan> _plans = [];

    public IReadOnlyList<DailyPlan> Plans => _plans;

    /// <summary>When set, the next <see cref="Add"/> call throws this instead of tracking the plan —
    /// simulates losing the DB's unique-index race between a concurrent scheduled-sweep run and the
    /// lazy-fallback endpoint.</summary>
    public Exception? ThrowOnNextAdd { get; set; }

    /// <summary>When <see cref="ThrowOnNextAdd"/> fires, this plan is inserted as a side effect first —
    /// simulating the concurrent process that actually won the DB's unique-index race and whose row a
    /// subsequent <see cref="GetByUserAndDateAsync"/> would now find.</summary>
    public DailyPlan? WinnerToInsertOnRace { get; set; }

    public void Seed(DailyPlan plan) => _plans.Add(plan);

    public Task<DailyPlan?> GetByIdAsync(DailyPlanId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_plans.FirstOrDefault(p => p.Id == id));

    public void Add(DailyPlan aggregate)
    {
        if (ThrowOnNextAdd is { } exception)
        {
            ThrowOnNextAdd = null;
            if (WinnerToInsertOnRace is { } winner)
            {
                _plans.Add(winner);
            }

            throw exception;
        }

        _plans.Add(aggregate);
    }

    public void Remove(DailyPlan aggregate) => _plans.Remove(aggregate);

    public Task<DailyPlan?> GetByUserAndDateAsync(
        Guid tenantId, Guid userId, DateOnly planDate, CancellationToken cancellationToken = default) =>
        Task.FromResult(_plans.FirstOrDefault(p => p.TenantId == tenantId && p.UserId == userId && p.PlanDate == planDate));
}
