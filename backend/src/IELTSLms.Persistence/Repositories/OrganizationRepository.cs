using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Persistence.Repositories;

internal sealed class OrganizationRepository(ApplicationDbContext dbContext) 
    : RepositoryBase<Organization, OrganizationId>(dbContext), IOrganizationRepository
{
    public async Task<Organization?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Name == name, cancellationToken);
    }
}
