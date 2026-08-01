using IELTSLms.Domain.Assessment;

namespace IELTSLms.Domain.Certificates;

/// <summary>
/// Evaluates <see cref="EligibleForCertificateSpecification"/> against a finalized
/// <see cref="MockExamResult"/> (DDD §10.1 Domain Services, sprint requirement: "keep eligibility
/// rules in this one service, don't scatter 'did they qualify' checks across endpoints"). The
/// qualifying band threshold isn't specified anywhere in the sprint doc or ERD beyond "issued only
/// when criteria met" (INV-06); <see cref="EligibleForCertificateSpecification.QualifyingOverallBand"/>
/// is this service's own placeholder business rule.
/// </summary>
public static class CertificateEligibilityService
{
    public static bool IsEligible(MockExamResult result) => EligibleForCertificateSpecification.IsSatisfiedBy(result);
}

/// <summary>
/// Specification pattern (DDD §7 Tactical Design Building Blocks) for certificate eligibility.
/// ponytail: a single fixed overall-band threshold, no per-tenant/per-course configurability — the
/// simplest rule that satisfies INV-06 ("issued only when criteria met"); upgrade to a configurable
/// <c>SystemSettings</c>-backed threshold if a real qualifying bar is defined later.
/// </summary>
public static class EligibleForCertificateSpecification
{
    public const decimal QualifyingOverallBand = 5.0m;

    public static bool IsSatisfiedBy(MockExamResult result) =>
        result.IsFinalized && result.OverallBand is { } band && band >= QualifyingOverallBand;
}
