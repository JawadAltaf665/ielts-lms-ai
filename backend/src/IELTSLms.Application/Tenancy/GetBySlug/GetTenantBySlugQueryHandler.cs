using IELTSLms.Abstractions.Persistence;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tenancy.GetBySlug;

public sealed class GetTenantBySlugQueryHandler(ITenantRepository tenants)
    : IQueryHandler<GetTenantBySlugQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantBySlugQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);

        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(TenantErrors.NotFound);
        }

        return Result.Success(TenantResponse.FromDomain(tenant));
    }
}
