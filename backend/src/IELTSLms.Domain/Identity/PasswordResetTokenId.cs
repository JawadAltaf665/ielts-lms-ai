namespace IELTSLms.Domain.Identity;

/// <summary>Strongly-typed identifier for a <see cref="PasswordResetToken"/>.</summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct PasswordResetTokenId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static PasswordResetTokenId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
