namespace IELTSLms.Domain.Tenancy;

/// <summary>Strongly-typed identifier for a <see cref="Tenant"/>.</summary>
/// <param name="Value">The underlying surrogate key.</param>
public readonly record struct TenantId(Guid Value)
{
    /// <summary>Creates an id from an existing GUID.</summary>
    public static TenantId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
