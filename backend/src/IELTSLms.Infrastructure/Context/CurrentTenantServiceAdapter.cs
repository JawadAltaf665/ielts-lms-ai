using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Tenancy;

namespace IELTSLms.Infrastructure.Context;

internal sealed class CurrentTenantServiceAdapter(ITenantContext tenantContext) : ICurrentTenantService
{
    public Guid? TenantId => tenantContext.TenantId;
    public string? TenantSlug => null; // Resolved in a later sprint when tenant metadata is loaded
}
