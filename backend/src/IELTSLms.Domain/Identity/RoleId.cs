namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="Role"/>.
/// </summary>
public readonly record struct RoleId(Guid Value)
{
    public static RoleId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
