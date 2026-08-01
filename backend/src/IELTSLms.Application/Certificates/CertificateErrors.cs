using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Certificates;

internal static class CertificateErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "NOT_FOUND_CERTIFICATE", "Certificate not found.");
}
