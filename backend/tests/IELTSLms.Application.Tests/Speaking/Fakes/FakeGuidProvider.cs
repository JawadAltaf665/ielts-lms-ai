using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeGuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}
