using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.Billing.GetInvoices;

/// <summary>Invoice search/history for the calling tenant (ADR-032, Sprint 2 §1).</summary>
public sealed record GetInvoicesQuery(Guid TenantId, string? Status, int Page, int PageSize) : IQuery<PagedList<InvoiceSummaryResponse>>;

public sealed record InvoiceSummaryResponse(
    Guid Id, string Number, string Status, string Currency, long TotalMinorUnits,
    DateTimeOffset? IssuedAtUtc, DateTimeOffset? PaidAtUtc, string? HostedInvoiceUrl);
