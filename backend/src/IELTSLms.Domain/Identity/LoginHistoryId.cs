namespace IELTSLms.Domain.Identity;

/// <summary>Strongly-typed identifier for a <see cref="LoginHistory"/> record.</summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct LoginHistoryId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static LoginHistoryId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
