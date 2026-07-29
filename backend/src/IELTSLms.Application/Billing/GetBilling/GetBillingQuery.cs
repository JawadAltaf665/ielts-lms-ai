using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetBilling;

/// <summary>Tenant-facing billing summary: customer, payment methods, and recent invoices (13 §15).</summary>
public sealed record GetBillingQuery(Guid TenantId) : IQuery<BillingSummaryResponse>;

public sealed record BillingSummaryResponse(
    Guid? CustomerId,
    string? Email,
    string? Name,
    IReadOnlyList<PaymentMethodResponse> PaymentMethods,
    IReadOnlyList<InvoiceSummaryResponse> RecentInvoices);

public sealed record PaymentMethodResponse(Guid Id, string Brand, string Last4, int ExpMonth, int ExpYear, bool IsDefault);

public sealed record InvoiceSummaryResponse(
    Guid Id, string Number, string Status, string Currency, long TotalMinorUnits, DateTimeOffset? IssuedAtUtc, DateTimeOffset? PaidAtUtc);
