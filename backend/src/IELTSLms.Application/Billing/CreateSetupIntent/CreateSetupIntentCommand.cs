using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.CreateSetupIntent;

/// <summary>
/// Starts collecting a new payment method client-side (Stripe Elements/Stripe.js) — the card never
/// reaches our server, satisfying PCI scope (Sprint 2 §3, §18).
/// </summary>
public sealed record CreateSetupIntentCommand(Guid TenantId, string Email, string? Name) : ICommand<SetupIntentResponse>;

public sealed record SetupIntentResponse(string ClientSecret);
