using IELTSLms.Domain.Certificates;
using IELTSLms.Domain.Certificates.Events;

namespace IELTSLms.Domain.Tests.Certificates;

public sealed class CertificateTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Certificate Issue(out Guid tenantId, out Guid userId)
    {
        tenantId = Guid.NewGuid();
        userId = Guid.NewGuid();
        return Certificate.Issue(
            CertificateId.From(Guid.NewGuid()), tenantId, userId, CertificateSourceType.MockExamResult, Guid.NewGuid(), 6.5m,
            "IELTS-ABC123", Now);
    }

    [Fact]
    public void Issue_starts_issued_and_raises_CertificateIssued()
    {
        var certificate = Issue(out var tenantId, out var userId);

        Assert.Equal(CertificateStatus.Issued, certificate.Status);
        var raised = Assert.Single(certificate.DomainEvents);
        var issued = Assert.IsType<CertificateIssuedDomainEvent>(raised);
        Assert.Equal(tenantId, issued.TenantId);
        Assert.Equal(userId, issued.UserId);
        Assert.Equal(certificate.Id.Value, issued.CertificateId);
        Assert.Equal("IELTS-ABC123", issued.VerificationCode);
    }

    [Fact]
    public void AttachPdf_records_the_storage_key()
    {
        var certificate = Issue(out _, out _);

        certificate.AttachPdf("certificates/tenant/cert.pdf", Now.AddMinutes(1));

        Assert.Equal("certificates/tenant/cert.pdf", certificate.PdfStorageKey);
    }

    [Fact]
    public void Revoke_transitions_to_revoked()
    {
        var certificate = Issue(out _, out _);

        var result = certificate.Revoke("Issued in error.", Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(CertificateStatus.Revoked, certificate.Status);
        Assert.Equal("Issued in error.", certificate.RevokedReason);
    }

    [Fact]
    public void Revoking_an_already_revoked_certificate_fails()
    {
        var certificate = Issue(out _, out _);
        certificate.Revoke("first", Now.AddDays(1));

        var result = certificate.Revoke("second", Now.AddDays(2));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_CERTIFICATE_ALREADY_REVOKED", result.Error.Code);
    }
}
