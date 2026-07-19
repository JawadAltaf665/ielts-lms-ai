namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// The lifecycle state of a <see cref="Tenant"/>, per ERD ENT-001 (Multi-Tenant Architecture §4).
/// Soft-delete (closure) is tracked separately via <see cref="SharedKernel.Domain.ISoftDeletable"/>;
/// this enum captures only the operational state of a live tenant.
/// </summary>
public enum TenantStatus
{
    /// <summary>Fully operational; users may authenticate and operate within the tenant.</summary>
    Active = 1,

    /// <summary>Temporarily disabled (e.g. non-payment, policy hold); access is denied but data is retained.</summary>
    Suspended = 2,

    /// <summary>Wound down; the tenant is closed and awaiting retention-driven purge.</summary>
    Closed = 3,
}
