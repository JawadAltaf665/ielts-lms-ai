using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Restore;

public sealed record RestoreTenantCommand(Guid TenantId) : ICommand;
