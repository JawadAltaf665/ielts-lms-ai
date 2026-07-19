namespace IELTSLms.Abstractions.Time;

/// <summary>
/// Abstraction over identifier generation so entity creation is deterministic under test.
/// Implementations should produce sequential (version-7) GUIDs to keep database indexes friendly.
/// </summary>
public interface IGuidProvider
{
    /// <summary>Creates a new GUID (implementation chooses the generation strategy).</summary>
    Guid NewGuid();
}
