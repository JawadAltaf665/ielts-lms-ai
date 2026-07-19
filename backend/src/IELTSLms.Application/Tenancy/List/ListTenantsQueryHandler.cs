using IELTSLms.Abstractions.Persistence;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tenancy.List;

public sealed class ListTenantsQueryHandler(ITenantRepository tenants)
    : IQueryHandler<ListTenantsQuery, ListTenantsResponse>
{
    public async Task<Result<ListTenantsResponse>> Handle(
        ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await tenants.ListAsync(skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var totalCount = await tenants.CountAsync(cancellationToken).ConfigureAwait(false);

        var response = new ListTenantsResponse(
            items.Select(TenantResponse.FromDomain).ToList(),
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(response);
    }
}
