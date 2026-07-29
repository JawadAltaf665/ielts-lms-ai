using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeGuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}
