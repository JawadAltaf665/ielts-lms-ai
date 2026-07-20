using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Tenancy.List;

public sealed record ListTenantsQuery(int Page, int PageSize) : IQuery<ListTenantsResponse>;

public sealed record ListTenantsResponse(
    IReadOnlyList<TenantResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
