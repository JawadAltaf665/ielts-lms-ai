using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Domain.Certificates;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Certificates;

internal sealed class CertificateRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Certificate, CertificateId>(dbContext), ICertificateRepository
{
    /// <summary>Deliberately cross-tenant (API Design Standards §16's one public exception, ERD ENT-038)
    /// — bypasses the tenant global filter the same way <c>InvoiceRepository.ListPaidBetweenAsync</c>
    /// does for its own cross-tenant scheduled-job read, and re-applies the soft-delete filter by hand
    /// since <see cref="EntityFrameworkQueryableExtensions.IgnoreQueryFilters{TEntity}"/> drops both filters at once.</summary>
    public async Task<Certificate?> GetByVerificationCodeAsync(string verificationCode, CancellationToken cancellationToken = default) =>
        await Set.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<Certificate?> GetBySourceResultAsync(
        Guid tenantId, CertificateSourceType sourceType, Guid sourceResultId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.SourceType == sourceType && c.SourceResultId == sourceResultId, cancellationToken)
            .ConfigureAwait(false);
}
