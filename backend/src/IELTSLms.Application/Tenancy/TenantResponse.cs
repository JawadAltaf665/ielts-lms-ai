using IELTSLms.Domain.Tenancy;

namespace IELTSLms.Application.Tenancy;

public sealed record TenantResponse(
    Guid Id,
    string Slug,
    string Code,
    string Name,
    string Status,
    string? PrimaryDomain,
    string? ContactEmail,
    string DefaultLocale,
    string DefaultTimeZone,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static TenantResponse FromDomain(Tenant tenant) => new(
        tenant.Id.Value,
        tenant.Slug.Value,
        tenant.Code.Value,
        tenant.Name,
        tenant.Status.ToString(),
        tenant.PrimaryDomain,
        tenant.ContactEmail,
        tenant.DefaultLocale,
        tenant.DefaultTimeZone,
        tenant.CreatedAtUtc,
        tenant.UpdatedAtUtc);
}
