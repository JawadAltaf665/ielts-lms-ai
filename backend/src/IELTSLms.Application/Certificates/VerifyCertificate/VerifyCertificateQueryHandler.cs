using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Certificates.VerifyCertificate;

public sealed class VerifyCertificateQueryHandler(ICertificateRepository certificates)
    : IQueryHandler<VerifyCertificateQuery, CertificateVerificationResponse>
{
    public async Task<Result<CertificateVerificationResponse>> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
    {
        var certificate = await certificates
            .GetByVerificationCodeAsync(request.VerificationCode, cancellationToken)
            .ConfigureAwait(false);
        if (certificate is null)
        {
            return Result.Failure<CertificateVerificationResponse>(CertificateErrors.NotFound);
        }

        return Result.Success(new CertificateVerificationResponse(
            certificate.VerificationCode, certificate.Status.ToString(), certificate.OverallBand, certificate.IssuedAtUtc));
    }
}
