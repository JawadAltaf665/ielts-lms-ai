using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Persistence.Repositories;

internal sealed class KnownDeviceRepository(ApplicationDbContext dbContext) 
    : RepositoryBase<KnownDevice, KnownDeviceId>(dbContext), IKnownDeviceRepository
{
    public async Task<KnownDevice?> GetByIdentifierAsync(UserId userId, string deviceIdentifier, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceIdentifier == deviceIdentifier, cancellationToken);
    }

    public async Task<List<KnownDevice>> GetUserDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await Set.Where(d => d.UserId == userId).ToListAsync(cancellationToken);
    }
}
