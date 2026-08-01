using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}
