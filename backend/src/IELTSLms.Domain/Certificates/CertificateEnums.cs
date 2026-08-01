namespace IELTSLms.Domain.Certificates;

/// <summary>Lifecycle of a <see cref="Certificate"/> (ERD ENT-038). Revocation is a state, not a deletion.</summary>
public enum CertificateStatus
{
    Issued = 1,
    Revoked = 2,
}

/// <summary>The kind of result a <see cref="Certificate"/> was issued from. Only mock-exam completion
/// is wired to automatic issuance this sprint (<c>CertificateEligibilityService</c>); the enum leaves
/// room for other qualifying sources without a schema change.</summary>
public enum CertificateSourceType
{
    MockExamResult = 1,
}

/// <summary>Well-known <see cref="Achievement"/> type codes. Deliberately small — this sprint only
/// wires idempotent awarding for certificate issuance (ERD ENT-054); a fuller badge catalog is future scope.</summary>
public static class AchievementType
{
    public const string CertificateEarned = "certificate-earned";
}
