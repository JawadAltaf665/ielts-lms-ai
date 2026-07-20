namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marks an entity using optimistic concurrency via PostgreSQL xmin.
/// </summary>
public interface IConcurrencyAware
{
    uint Version { get; }
}
