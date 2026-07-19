namespace IELTSLms.Api.Contracts;

public sealed record CreateTenantRequest(
    string Slug,
    string Code,
    string Name,
    string? PrimaryDomain,
    string? ContactEmail,
    string? DefaultLocale,
    string? DefaultTimeZone);

public sealed record UpdateTenantRequest(
    string Name,
    string? PrimaryDomain,
    string? ContactEmail,
    string? DefaultLocale,
    string? DefaultTimeZone);
