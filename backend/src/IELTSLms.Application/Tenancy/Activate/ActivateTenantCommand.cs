using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Activate;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
