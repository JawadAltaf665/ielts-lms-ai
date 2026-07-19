using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Update;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string? PrimaryDomain,
    string? ContactEmail,
    string? DefaultLocale,
    string? DefaultTimeZone) : ICommand;
