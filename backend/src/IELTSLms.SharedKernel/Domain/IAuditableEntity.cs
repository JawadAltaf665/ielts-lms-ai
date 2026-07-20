namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marks an entity that records when it was created and last modified. Timestamps are set by the
/// domain through its own methods (passing an explicit UTC instant), not by hidden statics
/// (Development Standards §10). The persistence layer maps these to audit columns
/// (ERD §4 audit fields).
/// </summary>
public interface IAuditableEntity
{
    /// <summary>When the entity was created (UTC).</summary>
    DateTimeOffset CreatedAtUtc { get; }

    /// <summary>When the entity was last modified (UTC), or <see langword="null"/> if never modified.</summary>
    DateTimeOffset? UpdatedAtUtc { get; }
}
