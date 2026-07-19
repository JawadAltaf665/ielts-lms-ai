using IELTSLms.Abstractions.Time;

namespace IELTSLms.Infrastructure.Time;

/// <summary>
/// Default <see cref="IGuidProvider"/> that emits version-7 (time-ordered) GUIDs, which keep
/// clustered database indexes sequential and reduce page fragmentation.
/// </summary>
internal sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.CreateVersion7();
}
