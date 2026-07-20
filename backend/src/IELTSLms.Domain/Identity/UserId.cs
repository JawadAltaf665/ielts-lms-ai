namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="User"/>. Wrapping the underlying <see cref="Guid"/>
/// prevents mixing ids of different aggregates at compile time (Development Standards §15).
/// </summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct UserId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID (e.g. rehydration or a provided value).</summary>
    public static UserId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
