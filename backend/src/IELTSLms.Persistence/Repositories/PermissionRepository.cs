using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Persistence.Repositories;

internal sealed class PermissionRepository(ApplicationDbContext dbContext) 
    : RepositoryBase<Permission, PermissionId>(dbContext), IPermissionRepository
{
    public async Task<Permission?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
    }

    public async Task<List<Permission>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Set.Where(p => p.Status == PermissionStatus.Active).ToListAsync(cancellationToken);
    }
}
