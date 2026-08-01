using IELTSLms.Domain.Certificates.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Certificates;

/// <summary>
/// A verifiable achievement certificate (ERD ENT-038, DDD §13.16 BC-16). Issued only when
/// <c>CertificateEligibilityService</c> confirms the source result satisfies
/// <c>EligibleForCertificateSpecification</c> (INV-06) — this aggregate itself only enforces the
/// verification-code invariant and the Issued/Revoked state machine; eligibility lives in the domain
/// service, not here (sprint requirement: keep "did they qualify" in one place). Verification is
/// deliberately public/unauthenticated (API Design Standards §16, ERD ENT-038) — <see cref="VerificationCode"/>
/// is globally unique and safe to expose since it grants read-only, non-sensitive lookup only.
/// </summary>
public sealed class Certificate : TenantAuditableEntity<CertificateId>
{
    private Certificate(
        CertificateId id, Guid tenantId, Guid userId, CertificateSourceType sourceType, Guid sourceResultId,
        decimal overallBand, string verificationCode, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SourceType = sourceType;
        SourceResultId = sourceResultId;
        OverallBand = overallBand;
        VerificationCode = verificationCode;
        Status = CertificateStatus.Issued;
        IssuedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private Certificate()
    {
        VerificationCode = null!;
    }

    public Guid UserId { get; private init; }

    public CertificateSourceType SourceType { get; private init; }

    /// <summary>Identifier of the qualifying result (e.g. a <c>MockExamResultId</c>). Soft cross-aggregate reference — no FK.</summary>
    public Guid SourceResultId { get; private init; }

    /// <summary>Snapshot of the overall band at issuance time — printed on the certificate, never recomputed later.</summary>
    public decimal OverallBand { get; private init; }

    /// <summary>Globally unique verification code (ERD ENT-038) — the only field the public verify endpoint looks up by.</summary>
    public string VerificationCode { get; private init; }

    public CertificateStatus Status { get; private set; }

    public DateTimeOffset IssuedAtUtc { get; private init; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedReason { get; private set; }

    /// <summary>Object-storage key for the generated PDF (<c>IFileStorage</c>), set once at issuance. Never returned to a client directly — resolved via a signed URL like every other stored file.</summary>
    public string? PdfStorageKey { get; private set; }

    public static Certificate Issue(
        CertificateId id, Guid tenantId, Guid userId, CertificateSourceType sourceType, Guid sourceResultId,
        decimal overallBand, string verificationCode, DateTimeOffset nowUtc)
    {
        var certificate = new Certificate(id, tenantId, userId, sourceType, sourceResultId, overallBand, verificationCode, nowUtc);
        certificate.RaiseDomainEvent(new CertificateIssuedDomainEvent(tenantId, userId, id.Value, verificationCode));
        return certificate;
    }

    public void AttachPdf(string storageKey, DateTimeOffset nowUtc)
    {
        PdfStorageKey = storageKey;
        UpdatedAtUtc = nowUtc;
    }

    public Result Revoke(string reason, DateTimeOffset nowUtc)
    {
        if (Status == CertificateStatus.Revoked)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_CERTIFICATE_ALREADY_REVOKED", "This certificate has already been revoked."));
        }

        Status = CertificateStatus.Revoked;
        RevokedReason = reason;
        RevokedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
