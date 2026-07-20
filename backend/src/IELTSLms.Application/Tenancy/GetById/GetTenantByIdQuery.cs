using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.GetById;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantResponse>;
