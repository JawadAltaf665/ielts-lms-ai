using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>Display grouping for related <see cref="Feature"/>s (ERD ENT-039D).</summary>
public sealed class FeatureGroup : AuditableEntity<FeatureGroupId>
{
    private FeatureGroup(FeatureGroupId id, string code, string name, int displayOrder, DateTimeOffset nowUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        DisplayOrder = displayOrder;
        CreatedAtUtc = nowUtc;
    }

    private FeatureGroup()
    {
        Code = null!;
        Name = null!;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public int DisplayOrder { get; private set; }

    public static FeatureGroup Create(FeatureGroupId id, string code, string name, int displayOrder, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new FeatureGroup(id, code.Trim(), name.Trim(), displayOrder, nowUtc);
    }
}
