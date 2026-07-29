using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Internal commercial tier referencing a <see cref="Product"/> (ERD ENT-039B, ADR-031).
/// <see cref="DisplayOrder"/> encodes the upgrade (higher order) / downgrade (lower order) direction.
/// </summary>
public sealed class Plan : AuditableEntity<PlanId>
{
    private Plan(
        PlanId id, ProductId productId, PlanCode code, string name, int displayOrder,
        bool trialEligible, int trialDays, DateTimeOffset nowUtc)
        : base(id)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        DisplayOrder = displayOrder;
        TrialEligible = trialEligible;
        TrialDays = trialDays;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private Plan()
    {
        Name = null!;
    }

    public const int NameMaxLength = 256;

    public ProductId ProductId { get; private set; }

    public PlanCode Code { get; private set; }

    public string Name { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool TrialEligible { get; private set; }

    public int TrialDays { get; private set; }

    public bool IsActive { get; private set; }

    public static Result<Plan> Create(
        PlanId id, ProductId productId, PlanCode code, string name, int displayOrder,
        bool trialEligible, int trialDays, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Plan name is required.");
        }

        if (trialDays < 0)
        {
            return Error.Validation("VALIDATION_TRIAL_DAYS", "Trial days cannot be negative.");
        }

        return new Plan(id, productId, code, name.Trim(), displayOrder, trialEligible, trialDays, nowUtc);
    }

    /// <summary>Positive when <paramref name="other"/> is a higher tier (an upgrade target).</summary>
    public int CompareTierTo(Plan other) => DisplayOrder.CompareTo(other.DisplayOrder);

    public Result Deactivate(DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Activate(DateTimeOffset nowUtc)
    {
        if (IsActive)
        {
            return Result.Success();
        }

        IsActive = true;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
