using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.Delete;

public sealed record DeleteTenantCommand(Guid TenantId) : ICommand;
