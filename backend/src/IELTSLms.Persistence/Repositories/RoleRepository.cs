using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Persistence.Repositories;

internal sealed class RoleRepository(ApplicationDbContext dbContext) 
    : RepositoryBase<Role, RoleId>(dbContext), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);
    }

    public async Task<List<Role>> GetRolesForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Set.Where(r => r.TenantId == tenantId || r.TenantId == null).ToListAsync(cancellationToken);
    }
}
