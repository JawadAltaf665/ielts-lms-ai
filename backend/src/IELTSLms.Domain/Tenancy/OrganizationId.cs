namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// Strongly-typed identifier for an <see cref="Organization"/>.
/// </summary>
public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
