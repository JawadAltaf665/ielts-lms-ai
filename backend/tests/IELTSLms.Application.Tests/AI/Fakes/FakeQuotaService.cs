using IELTSLms.Abstractions.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeQuotaService : IQuotaService
{
    public bool Allowed { get; set; } = true;

    public int ConsumeCallCount { get; private set; }

    public Task<QuotaCheckResult> CheckAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default) =>
        Task.FromResult(Allowed ? new QuotaCheckResult(true, 100, 0, 100) : new QuotaCheckResult(false, 100, 100, 0));

    public Task<Result> ConsumeAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default)
    {
        ConsumeCallCount++;
        return Task.FromResult(Allowed
            ? Result.Success()
            : Result.Failure(Error.RateLimited("RATE_LIMITED_AI_QUOTA_EXCEEDED", "quota exceeded")));
    }
}
