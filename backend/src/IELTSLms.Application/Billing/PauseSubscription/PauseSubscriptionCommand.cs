using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.PauseSubscription;

/// <summary>Voluntary, tenant-initiated pause (ADR-032, Sprint 2 §11) — distinct from involuntary suspension.</summary>
public sealed record PauseSubscriptionCommand(Guid TenantId) : ICommand;
