namespace IELTSLms.Domain.Certificates;

// Strongly-typed identifiers for the Certificates bounded context (BC-16, ERD §6.4), grouped in one
// file per the Grammar/Billing convention (GrammarIds.cs) — one-file-per-id doesn't scale without pure ceremony.

/// <summary>Strongly-typed identifier for a <see cref="Certificate"/>.</summary>
public readonly record struct CertificateId(Guid Value)
{
    public static CertificateId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="Achievement"/>.</summary>
public readonly record struct AchievementId(Guid Value)
{
    public static AchievementId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
