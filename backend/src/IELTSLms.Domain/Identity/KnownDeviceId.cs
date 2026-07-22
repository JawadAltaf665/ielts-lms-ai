namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="KnownDevice"/>.
/// </summary>
public readonly record struct KnownDeviceId(Guid Value)
{
    public static KnownDeviceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
