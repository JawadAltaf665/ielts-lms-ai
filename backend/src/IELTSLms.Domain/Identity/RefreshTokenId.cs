namespace IELTSLms.Domain.Identity;

/// <summary>Strongly-typed identifier for a <see cref="RefreshToken"/>.</summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct RefreshTokenId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static RefreshTokenId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
