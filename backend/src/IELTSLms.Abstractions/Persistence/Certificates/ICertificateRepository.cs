using IELTSLms.Domain.Certificates;

namespace IELTSLms.Abstractions.Persistence.Certificates;

/// <summary>Aggregate access for <see cref="Certificate"/> (ERD ENT-038).</summary>
public interface ICertificateRepository : IRepository<Certificate, CertificateId>
{
    /// <summary>Looks up by the globally unique verification code — the sole access path for the public verify endpoint.</summary>
    Task<Certificate?> GetByVerificationCodeAsync(string verificationCode, CancellationToken cancellationToken = default);

    /// <summary>The certificate already issued for this exact source result, if any — the idempotency guard <c>CertificateIssuanceService</c> checks before issuing a duplicate.</summary>
    Task<Certificate?> GetBySourceResultAsync(
        Guid tenantId, CertificateSourceType sourceType, Guid sourceResultId, CancellationToken cancellationToken = default);
}
