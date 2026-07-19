using IELTSLms.Abstractions.Persistence;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tenancy.GetById;

public sealed class GetTenantByIdQueryHandler(ITenantRepository tenants)
    : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(
            TenantId.From(request.TenantId), cancellationToken).ConfigureAwait(false);

        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(TenantErrors.NotFound);
        }

        return Result.Success(TenantResponse.FromDomain(tenant));
    }
}
