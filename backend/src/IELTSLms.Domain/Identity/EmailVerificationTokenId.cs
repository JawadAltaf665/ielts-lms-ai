namespace IELTSLms.Domain.Identity;

/// <summary>Strongly-typed identifier for an <see cref="EmailVerificationToken"/>.</summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct EmailVerificationTokenId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static EmailVerificationTokenId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
