using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Domain.Certificates;

namespace IELTSLms.Application.Tests.Certificates.Fakes;

internal sealed class FakeCertificateRepository : ICertificateRepository
{
    private readonly List<Certificate> _certificates = [];

    public IReadOnlyList<Certificate> Certificates => _certificates;

    public void Seed(Certificate certificate) => _certificates.Add(certificate);

    public Task<Certificate?> GetByIdAsync(CertificateId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_certificates.FirstOrDefault(c => c.Id == id));

    public void Add(Certificate aggregate) => _certificates.Add(aggregate);

    public void Remove(Certificate aggregate) => _certificates.Remove(aggregate);

    public Task<Certificate?> GetByVerificationCodeAsync(string verificationCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(_certificates.FirstOrDefault(c => c.VerificationCode == verificationCode));

    public Task<Certificate?> GetBySourceResultAsync(
        Guid tenantId, CertificateSourceType sourceType, Guid sourceResultId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_certificates.FirstOrDefault(
            c => c.TenantId == tenantId && c.SourceType == sourceType && c.SourceResultId == sourceResultId));
}
