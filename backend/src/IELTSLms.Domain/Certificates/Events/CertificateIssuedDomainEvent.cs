using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Certificates.Events;

/// <summary>Raised when a <see cref="Certificate"/> is issued (DDD §13.16 BC-16, POL-06).</summary>
public sealed record CertificateIssuedDomainEvent(Guid TenantId, Guid UserId, Guid CertificateId, string VerificationCode)
    : DomainEvent;
