namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Configuration for host-based tenant resolution (Multi-Tenant Architecture §5.1, §8):
/// the platform's own root domain(s), so <see cref="HostTenantResolutionStrategy"/> can strip a
/// known suffix off the request <c>Host</c> header and treat what remains as the tenant slug
/// (<c>acme</c> from <c>acme.ielts-lms.ai</c>).
/// </summary>
public sealed class TenantResolutionOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Tenancy";

    /// <summary>
    /// Root domains the platform is served from. A request Host that is exactly one of these (the
    /// apex, no subdomain) or doesn't end in any of these resolves nothing via host — the header or
    /// JWT-claim strategies apply instead. Case-insensitive; a leading dot is optional.
    /// </summary>
    public string[] RootDomains { get; set; } = [];
}
