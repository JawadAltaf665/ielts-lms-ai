using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Learning.Fakes;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}
