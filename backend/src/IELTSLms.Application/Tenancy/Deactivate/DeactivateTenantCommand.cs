using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Deactivate;

public sealed record DeactivateTenantCommand(Guid TenantId) : ICommand;
