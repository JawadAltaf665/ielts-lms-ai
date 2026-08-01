using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Certificates.GetCertificate;

/// <summary>Gets certificate detail for its owner or a teacher (<c>certificate.view</c>).</summary>
public sealed record GetCertificateQuery(Guid TenantId, Guid UserId, Guid CertificateId) : IQuery<CertificateResponse>;

public sealed record CertificateResponse(
    Guid Id, Guid UserId, string SourceType, Guid SourceResultId, decimal OverallBand, string VerificationCode,
    string Status, DateTimeOffset IssuedAtUtc, DateTimeOffset? RevokedAtUtc, string? RevokedReason);
