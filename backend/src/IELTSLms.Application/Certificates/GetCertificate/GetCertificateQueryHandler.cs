using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Certificates;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Certificates.GetCertificate;

public sealed class GetCertificateQueryHandler(ICertificateRepository certificates, ICurrentUserService currentUser)
    : IQueryHandler<GetCertificateQuery, CertificateResponse>
{
    public async Task<Result<CertificateResponse>> Handle(GetCertificateQuery request, CancellationToken cancellationToken)
    {
        var certificate = await certificates
            .GetByIdAsync(CertificateId.From(request.CertificateId), cancellationToken)
            .ConfigureAwait(false);

        var isOwner = certificate is not null && certificate.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (certificate is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<CertificateResponse>(CertificateErrors.NotFound);
        }

        return Result.Success(ToResponse(certificate));
    }

    internal static CertificateResponse ToResponse(Certificate certificate) => new(
        certificate.Id.Value, certificate.UserId, certificate.SourceType.ToString(), certificate.SourceResultId,
        certificate.OverallBand, certificate.VerificationCode, certificate.Status.ToString(), certificate.IssuedAtUtc,
        certificate.RevokedAtUtc, certificate.RevokedReason);
}
