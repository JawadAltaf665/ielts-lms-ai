using IELTSLms.Abstractions.Tenancy;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeTenantContext(Guid? tenantId) : ITenantContext
{
    public Guid? TenantId { get; } = tenantId;
    public bool HasTenant => TenantId is not null;
    public bool IsBypassed => false;
}
