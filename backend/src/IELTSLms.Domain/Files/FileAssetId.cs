namespace IELTSLms.Domain.Files;

/// <summary>Strongly-typed identifier for a <see cref="FileAsset"/>.</summary>
public readonly record struct FileAssetId(Guid Value)
{
    public static FileAssetId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
