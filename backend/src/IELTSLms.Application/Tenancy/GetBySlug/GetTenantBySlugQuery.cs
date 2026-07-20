using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.GetBySlug;

public sealed record GetTenantBySlugQuery(string Slug) : IQuery<TenantResponse>;
