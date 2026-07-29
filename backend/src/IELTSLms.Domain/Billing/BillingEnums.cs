namespace IELTSLms.Domain.Billing;

/// <summary>Internal commercial tier codes (13 §2.1).</summary>
public enum PlanCode
{
    Free = 1,
    Starter = 2,
    Premium = 3,
    Institute = 4,
    Enterprise = 5,
}

/// <summary>Billing cadence for a <see cref="Price"/> (13 §5).</summary>
public enum BillingInterval
{
    Monthly = 1,
    Yearly = 2,
}

/// <summary>How often a <see cref="UsageRecord"/> resets for a <see cref="QuotaDefinition"/>.</summary>
public enum QuotaResetCycle
{
    Daily = 1,
    Monthly = 2,
    Never = 3,
}

/// <summary>Lifecycle state of a <see cref="Subscription"/> (13 §4, ERD ENT-040).</summary>
public enum SubscriptionStatus
{
    Pending = 1,
    Trial = 2,
    Active = 3,
    PastDue = 4,
    Suspended = 5,
    Cancelled = 6,
    Expired = 7,
}

/// <summary>Lifecycle state of an <see cref="Invoice"/> (13 §8.1, ERD ENT-041).</summary>
public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Paid = 3,
    Void = 4,
    Uncollectible = 5,
}

/// <summary>Lifecycle state of a <see cref="Payment"/> (ERD ENT-042).</summary>
public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4,
}
