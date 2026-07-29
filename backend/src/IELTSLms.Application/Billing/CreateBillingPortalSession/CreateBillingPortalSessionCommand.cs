using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.CreateBillingPortalSession;

/// <summary>
/// The primary self-service surface (ADR-032, Sprint 2 §4): plan changes, payment methods, invoice
/// download, and cancel/resume are all handled by Stripe's hosted portal.
/// </summary>
public sealed record CreateBillingPortalSessionCommand(Guid TenantId, string ReturnUrl) : ICommand<BillingPortalSessionResponse>;

public sealed record BillingPortalSessionResponse(string Url);
