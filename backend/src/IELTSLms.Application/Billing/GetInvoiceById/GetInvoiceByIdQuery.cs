using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetInvoices;

namespace IELTSLms.Application.Billing.GetInvoiceById;

/// <summary>A single invoice's detail, including the Stripe-hosted URL for "download" (ADR-032).</summary>
public sealed record GetInvoiceByIdQuery(Guid TenantId, Guid InvoiceId) : IQuery<InvoiceSummaryResponse>;
