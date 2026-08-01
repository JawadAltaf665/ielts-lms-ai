using IELTSLms.Abstractions.Tenancy;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeTenantContextAccessor : ITenantContextAccessor
{
    public Guid? TenantId { get; private set; }

    public bool HasTenant => TenantId is not null;

    public bool IsBypassed { get; private set; }

    public void SetTenant(Guid tenantId) => TenantId = tenantId;

    public IDisposable BypassTenantFilter()
    {
        IsBypassed = true;
        return new Restorer(this);
    }

    private sealed class Restorer(FakeTenantContextAccessor owner) : IDisposable
    {
        public void Dispose() => owner.IsBypassed = false;
    }
}
