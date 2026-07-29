using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.ProcessStripeWebhook;

/// <summary>
/// Applies a verified Stripe webhook event to local billing state (13 §11). The envelope has already
/// been signature-verified and unwrapped by <see cref="IELTSLms.Abstractions.Billing.IStripeWebhookVerifier"/>
/// at the API boundary — this command only sees the provider-agnostic event id/type/payload.
/// </summary>
public sealed record ProcessStripeWebhookCommand(string EventId, string EventType, string RawJson) : ICommand;
