using IELTSLms.Abstractions.Documents;

namespace IELTSLms.Application.Tests.Certificates.Fakes;

internal sealed class FakeCertificatePdfGenerator : ICertificatePdfGenerator
{
    public int GenerateCallCount { get; private set; }

    public byte[] Generate(CertificatePdfModel model)
    {
        GenerateCallCount++;
        return [1, 2, 3];
    }
}
