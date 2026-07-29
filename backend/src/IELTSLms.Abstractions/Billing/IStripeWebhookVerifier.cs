using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Abstractions.Billing;

/// <summary>
/// Verifies an inbound Stripe webhook's signature and unwraps its event envelope (13 §11).
/// Implemented against the raw request body — signature verification requires the exact bytes
/// Stripe signed, before any JSON model binding.
/// </summary>
public interface IStripeWebhookVerifier
{
    /// <summary>
    /// Verifies <paramref name="signatureHeader"/> (the <c>Stripe-Signature</c> header) against
    /// <paramref name="payload"/> (the raw request body) and returns the event envelope, or a
    /// validation <see cref="Error"/> if the signature does not match.
    /// </summary>
    Result<StripeEventEnvelope> ConstructEvent(string payload, string signatureHeader);
}

/// <summary>A verified Stripe webhook event, unwrapped to what the application layer needs.</summary>
/// <param name="EventId">Stripe's event id — the idempotency key (13 §11).</param>
/// <param name="EventType">The Stripe event type, e.g. <c>invoice.payment_succeeded</c>.</param>
/// <param name="RawJson">The event's <c>data.object</c> payload as raw JSON for type-specific deserialization.</param>
public sealed record StripeEventEnvelope(string EventId, string EventType, string RawJson);
