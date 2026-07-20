using IELTSLms.Abstractions.Tenancy;

namespace IELTSLms.Infrastructure.Tenancy;

/// <summary>
/// Scoped, mutable holder for the ambient tenant (Multi-Tenant Architecture §5.2, §7). One instance
/// lives per request scope: tenant-resolution middleware calls <see cref="SetTenant"/> once at the
/// edge, and every downstream layer reads the tenant through <see cref="ITenantContext"/>. Because it
/// is scoped it is confined to a single logical request and is not shared across threads; the
/// bypass stack guards privileged cross-tenant work and always restores prior state on dispose.
/// </summary>
internal sealed class TenantContext : ITenantContextAccessor
{
    private int _bypassDepth;

    /// <inheritdoc />
    public Guid? TenantId { get; private set; }

    /// <inheritdoc />
    public bool HasTenant => TenantId is not null;

    /// <inheritdoc />
    public bool IsBypassed => _bypassDepth > 0;

    /// <inheritdoc />
    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must not be empty.", nameof(tenantId));
        }

        if (TenantId is { } existing && existing != tenantId)
        {
            // Re-binding a different tenant mid-scope would breach the "resolve once" guarantee.
            throw new InvalidOperationException(
                "The tenant for this scope has already been established and cannot be reassigned.");
        }

        TenantId = tenantId;
    }

    /// <inheritdoc />
    public IDisposable BypassTenantFilter()
    {
        _bypassDepth++;
        return new BypassScope(this);
    }

    private void EndBypass() => _bypassDepth--;

    /// <summary>Restores the prior bypass state when disposed, so bypass cannot leak past its scope.</summary>
    private sealed class BypassScope(TenantContext owner) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner.EndBypass();
        }
    }
}
