namespace IELTSLms.Domain.Learning;

/// <summary>Strongly-typed identifier for a <see cref="ProgressTracking"/>.</summary>
public readonly record struct ProgressTrackingId(Guid Value)
{
    public static ProgressTrackingId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="WeaknessProfile"/> (Sprint 9, ERD ENT-NEW).</summary>
public readonly record struct WeaknessProfileId(Guid Value)
{
    public static WeaknessProfileId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="LearningRecommendation"/> (Sprint 9, ERD ENT-NEW).</summary>
public readonly record struct LearningRecommendationId(Guid Value)
{
    public static LearningRecommendationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="DailyPlan"/> (ERD ENT-034).</summary>
public readonly record struct DailyPlanId(Guid Value)
{
    public static DailyPlanId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
