namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="Permission"/>.
/// </summary>
public readonly record struct PermissionId(Guid Value)
{
    public static PermissionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
