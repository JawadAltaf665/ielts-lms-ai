using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.Billing.GetPayments;

/// <summary>Payment history for the calling tenant (Sprint 2 §2).</summary>
public sealed record GetPaymentsQuery(Guid TenantId, int Page, int PageSize) : IQuery<PagedList<PaymentSummaryResponse>>;

public sealed record PaymentSummaryResponse(
    Guid Id, Guid InvoiceId, string Status, long AmountMinorUnits, string Currency,
    string? FailureReason, DateTimeOffset? PaidAtUtc, DateTimeOffset CreatedAtUtc);
