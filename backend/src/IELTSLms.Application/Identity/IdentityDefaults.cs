namespace IELTSLms.Application.Identity;

/// <summary>
/// Interim identity constants for Sprint 2. Tenancy is not implemented yet (see the sprint scope):
/// all accounts are stamped with a single placeholder tenant so the <c>tenant_id</c> column and the
/// <c>(tenant_id, email)</c> uniqueness rule are exercised now, and Sprint 3 can introduce the real
/// Tenants table and resolve the tenant from context without a schema change.
/// </summary>
public static class IdentityDefaults
{
    /// <summary>The single placeholder tenant every account belongs to until real tenancy lands.</summary>
    public static readonly Guid TenantId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>The <c>amr</c> claim value for a password login (Auth §9.1).</summary>
    public const string PasswordAuthenticationMethod = "pwd";

    /// <summary>
    /// The JWT claim type carrying an assigned role name (Auth §9.1). A short custom claim, matching
    /// the token's other claims (<c>sub</c>, <c>tenantId</c>, <c>sid</c>, <c>amr</c>) rather than the
    /// verbose <see cref="System.Security.Claims.ClaimTypes.Role"/> URI.
    /// </summary>
    public const string RoleClaimType = "role";
}
