namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Base class for entities: identity-based equality. State is changed only through methods that
/// protect invariants (Development Standards §15).
/// </summary>
/// <typeparam name="TId">The identifier type (typically a strongly-typed id value object).</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    /// <summary>Required by EF Core materialization; not for domain use.</summary>
    protected Entity()
    {
        Id = default!;
    }

    public TId Id { get; protected init; }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
