using IELTSLms.Domain.Tenancy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Abstractions.Persistence;

public interface IOrganizationRepository : IRepository<Organization, OrganizationId>
{
    Task<Organization?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
}
