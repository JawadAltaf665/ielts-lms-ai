using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>Catalog of a meterable resource, e.g. AI Requests, Storage (ERD ENT-039H).</summary>
public sealed class QuotaDefinition : AuditableEntity<QuotaDefinitionId>
{
    private QuotaDefinition(
        QuotaDefinitionId id, string code, string name, string unit, QuotaResetCycle resetCycle, DateTimeOffset nowUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Unit = unit;
        ResetCycle = resetCycle;
        CreatedAtUtc = nowUtc;
    }

    private QuotaDefinition()
    {
        Code = null!;
        Name = null!;
        Unit = null!;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Unit { get; private set; }

    public QuotaResetCycle ResetCycle { get; private set; }

    public static QuotaDefinition Create(
        QuotaDefinitionId id, string code, string name, string unit, QuotaResetCycle resetCycle, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        return new QuotaDefinition(id, code.Trim(), name.Trim(), unit.Trim(), resetCycle, nowUtc);
    }
}
