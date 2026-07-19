using IELTSLms.Abstractions.Time;

namespace IELTSLms.Infrastructure.Time;

/// <summary>Default <see cref="IClock"/> backed by the machine clock, always in UTC.</summary>
internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
