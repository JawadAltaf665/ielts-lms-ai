using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Certificates.VerifyCertificate;

/// <summary>
/// Public, unauthenticated certificate verification by code (API Design Standards §16 — the phase's
/// one deliberate public/unauthenticated exception; ERD ENT-038). Deliberately cross-tenant: the
/// verification code alone is the lookup key, not scoped to a caller's tenant, since there is no
/// authenticated caller. The response is minimal/non-sensitive by construction (<see cref="CertificateVerificationResponse"/>
/// carries no <c>UserId</c>/<c>TenantId</c>/learner name) — never widen it without re-checking API §16.
/// </summary>
public sealed record VerifyCertificateQuery(string VerificationCode) : IQuery<CertificateVerificationResponse>;

public sealed record CertificateVerificationResponse(
    string VerificationCode, string Status, decimal OverallBand, DateTimeOffset IssuedAtUtc);
