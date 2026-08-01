using IELTSLms.Abstractions.Tenancy;

namespace IELTSLms.Application.Tests.Listening.Fakes;

internal sealed class FakeTenantContextAccessor : ITenantContextAccessor
{
    private int _bypassDepth;

    public Guid? TenantId { get; private set; }

    public bool HasTenant => TenantId is not null;

    public bool IsBypassed => _bypassDepth > 0;

    public void SetTenant(Guid tenantId) => TenantId = tenantId;

    public IDisposable BypassTenantFilter()
    {
        _bypassDepth++;
        return new BypassScope(this);
    }

    private sealed class BypassScope(FakeTenantContextAccessor owner) : IDisposable
    {
        public void Dispose() => owner._bypassDepth--;
    }
}
