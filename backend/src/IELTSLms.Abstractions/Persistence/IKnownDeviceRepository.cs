using IELTSLms.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Abstractions.Persistence;

public interface IKnownDeviceRepository : IRepository<KnownDevice, KnownDeviceId>
{
    Task<KnownDevice?> GetByIdentifierAsync(UserId userId, string deviceIdentifier, CancellationToken cancellationToken = default);
    Task<List<KnownDevice>> GetUserDevicesAsync(UserId userId, CancellationToken cancellationToken = default);
}
