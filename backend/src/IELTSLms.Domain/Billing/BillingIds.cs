namespace IELTSLms.Domain.Billing;

// Strongly-typed identifiers for the Billing bounded context (ADR-031). Grouped in one file since
// each is a trivial one-line wrapper — the Tenancy module's one-file-per-id convention doesn't scale
// to this many ids without pure ceremony.

/// <summary>Strongly-typed identifier for a <see cref="Product"/>.</summary>
public readonly record struct ProductId(Guid Value)
{
    public static ProductId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="Plan"/>.</summary>
public readonly record struct PlanId(Guid Value)
{
    public static PlanId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="Price"/>.</summary>
public readonly record struct PriceId(Guid Value)
{
    public static PriceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="FeatureGroup"/>.</summary>
public readonly record struct FeatureGroupId(Guid Value)
{
    public static FeatureGroupId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="Feature"/>.</summary>
public readonly record struct FeatureId(Guid Value)
{
    public static FeatureId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="PlanFeature"/>.</summary>
public readonly record struct PlanFeatureId(Guid Value)
{
    public static PlanFeatureId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="TenantFeature"/>.</summary>
public readonly record struct TenantFeatureId(Guid Value)
{
    public static TenantFeatureId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="QuotaDefinition"/>.</summary>
public readonly record struct QuotaDefinitionId(Guid Value)
{
    public static QuotaDefinitionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="PlanQuota"/>.</summary>
public readonly record struct PlanQuotaId(Guid Value)
{
    public static PlanQuotaId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="TenantQuota"/>.</summary>
public readonly record struct TenantQuotaId(Guid Value)
{
    public static TenantQuotaId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="UsageRecord"/>.</summary>
public readonly record struct UsageRecordId(Guid Value)
{
    public static UsageRecordId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="BillingCustomer"/>.</summary>
public readonly record struct BillingCustomerId(Guid Value)
{
    public static BillingCustomerId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="PaymentMethod"/>.</summary>
public readonly record struct PaymentMethodId(Guid Value)
{
    public static PaymentMethodId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="Subscription"/>.</summary>
public readonly record struct SubscriptionId(Guid Value)
{
    public static SubscriptionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="SubscriptionItem"/>.</summary>
public readonly record struct SubscriptionItemId(Guid Value)
{
    public static SubscriptionItemId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="Invoice"/>.</summary>
public readonly record struct InvoiceId(Guid Value)
{
    public static InvoiceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="InvoiceLine"/>.</summary>
public readonly record struct InvoiceLineId(Guid Value)
{
    public static InvoiceLineId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="Payment"/>.</summary>
public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
