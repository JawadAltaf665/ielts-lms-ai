using IELTSLms.Abstractions.Billing;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeEntitlementService : IEntitlementService
{
    public bool HasFeature { get; set; } = true;

    public Task<bool> HasFeatureAsync(Guid tenantId, string featureCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(HasFeature);

    public Task<IReadOnlyList<string>> ListEnabledFeatureCodesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<string>>(HasFeature ? ["ai.grammar-coach"] : []);
}
