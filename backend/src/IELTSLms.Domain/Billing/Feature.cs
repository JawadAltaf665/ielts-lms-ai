using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>A gateable platform capability, e.g. AI Tutor, Mock Exams (ERD ENT-039E).</summary>
public sealed class Feature : AuditableEntity<FeatureId>
{
    private Feature(
        FeatureId id, string code, string name, string? description, FeatureGroupId? featureGroupId, DateTimeOffset nowUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        FeatureGroupId = featureGroupId;
        CreatedAtUtc = nowUtc;
    }

    private Feature()
    {
        Code = null!;
        Name = null!;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public FeatureGroupId? FeatureGroupId { get; private set; }

    public static Feature Create(
        FeatureId id, string code, string name, string? description, FeatureGroupId? featureGroupId, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Feature(id, code.Trim(), name.Trim(), description, featureGroupId, nowUtc);
    }
}
