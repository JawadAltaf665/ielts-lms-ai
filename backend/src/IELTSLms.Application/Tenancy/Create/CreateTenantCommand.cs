using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Create;

public sealed record CreateTenantCommand(
    string Slug,
    string Code,
    string Name,
    string? PrimaryDomain,
    string? ContactEmail,
    string? DefaultLocale,
    string? DefaultTimeZone) : ICommand<Guid>;
