namespace IELTSLms.Abstractions.Documents;

/// <summary>
/// Renders a certificate PDF (sprint requirement: "use whatever .NET PDF library is already a project
/// dependency, or the smallest well-maintained one needed — this is a solved problem, don't build a PDF
/// renderer"). Implemented in Infrastructure against QuestPDF so Application/Domain stay free of the
/// rendering library (Backend Architecture §5.4 ACL pattern, same shape as <c>IAiGateway</c>).
/// </summary>
public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificatePdfModel model);
}

/// <summary>Everything the PDF template needs — a flat snapshot, not the domain entity, so rendering never depends on Domain.</summary>
public sealed record CertificatePdfModel(
    Guid CertificateId, string LearnerName, decimal OverallBand, string VerificationCode, DateTimeOffset IssuedAtUtc);
