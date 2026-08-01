using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Reading.Fakes;

internal sealed class FakeGuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}
