namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="UserSession"/>. This value is surfaced as the
/// <c>sid</c> claim in access tokens (Auth §9.1).
/// </summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct UserSessionId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static UserSessionId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
