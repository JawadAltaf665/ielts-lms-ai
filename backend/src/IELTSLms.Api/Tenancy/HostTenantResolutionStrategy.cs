using IELTSLms.Abstractions.Persistence;
using Microsoft.Extensions.Options;

namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the request's <c>Host</c> header (Multi-Tenant Architecture §5.1, §8):
/// a subdomain of a configured platform root domain (<c>acme.ielts-lms.ai</c> → slug <c>acme</c>), or
/// a tenant's mapped custom domain. This is the documented primary mechanism for browser traffic —
/// branding, tenant landing, and login/registration routing all resolve this way before a token
/// exists. It is ordered ahead of the header strategy (advisory, server-to-server only) but behind
/// the JWT claim strategy (authoritative once authenticated).
/// </summary>
public sealed class HostTenantResolutionStrategy(
    ITenantRepository tenants,
    IOptions<TenantResolutionOptions> options) : ITenantResolutionStrategy
{
    private readonly TenantResolutionOptions _options = options.Value;

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public string Name => "host";

    /// <inheritdoc />
    public async Task<Guid?> Resolve(HttpContext httpContext)
    {
        // HostString.Host is the hostname with any :port stripped.
        var host = httpContext.Request.Host.Host;
        if (string.IsNullOrEmpty(host))
        {
            return null;
        }

        host = host.ToLowerInvariant();

        var label = TryExtractSubdomainLabel(host);
        if (label is not null)
        {
            var bySlug = await tenants.GetBySlugAsync(label, httpContext.RequestAborted).ConfigureAwait(false);
            return bySlug?.Id.Value;
        }

        // Not a subdomain of a known root domain — the host might still be a tenant's own custom
        // domain (§8 Tenant Configuration, Branding & Domains).
        var byDomain = await tenants.GetByPrimaryDomainAsync(host, httpContext.RequestAborted).ConfigureAwait(false);
        return byDomain?.Id.Value;
    }

    /// <summary>
    /// Strips a configured root domain suffix off <paramref name="host"/> and returns the remaining
    /// single-label subdomain, or <see langword="null"/> if the host is the bare root domain (apex,
    /// no tenant), doesn't end in any configured root domain, or the remainder isn't a single label
    /// (multi-level subdomains aren't a supported tenant slug shape — <see cref="Domain.Tenancy.TenantSlug"/>).
    /// </summary>
    private string? TryExtractSubdomainLabel(string host)
    {
        foreach (var root in _options.RootDomains)
        {
            var normalizedRoot = root.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(normalizedRoot) || host == normalizedRoot)
            {
                continue;
            }

            var suffix = "." + normalizedRoot;
            if (!host.EndsWith(suffix, StringComparison.Ordinal))
            {
                continue;
            }

            var label = host[..^suffix.Length];
            if (label.Length > 0 && !label.Contains('.', StringComparison.Ordinal))
            {
                return label;
            }
        }

        return null;
    }
}
