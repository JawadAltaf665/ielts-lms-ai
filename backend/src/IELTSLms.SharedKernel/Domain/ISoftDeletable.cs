namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marks an entity that is removed logically rather than physically: a soft delete sets a flag and
/// timestamp so the row is retained for audit/history while being excluded from normal queries
/// (Development Standards §15, ERD §4 lifecycle). The persistence layer applies a global query
/// filter so soft-deleted rows are hidden by default.
/// </summary>
public interface ISoftDeletable
{
    /// <summary><see langword="true"/> once the entity has been soft-deleted.</summary>
    bool IsDeleted { get; }

    /// <summary>When the entity was soft-deleted (UTC), or <see langword="null"/> if still active.</summary>
    DateTimeOffset? DeletedAtUtc { get; }
}
