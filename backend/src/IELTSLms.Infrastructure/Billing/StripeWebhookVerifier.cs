using IELTSLms.Abstractions.Billing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Options;
using Stripe;

namespace IELTSLms.Infrastructure.Billing;

/// <summary>
/// Verifies Stripe webhook signatures via the SDK's <see cref="EventUtility"/> (13 §11). Requires the
/// raw request body — a mismatched/missing signature returns a validation <see cref="Error"/> rather
/// than throwing, so the endpoint can respond with a normal problem-details error.
/// </summary>
public sealed class StripeWebhookVerifier(IOptions<StripeOptions> options) : IStripeWebhookVerifier
{
    public Result<StripeEventEnvelope> ConstructEvent(string payload, string signatureHeader)
    {
        var webhookSecret = options.Value.WebhookSigningSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return Error.Failure("BUSINESS_STRIPE_NOT_CONFIGURED", "Stripe webhook signing secret is not configured.");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, webhookSecret);
            var rawJson = stripeEvent.Data.RawObject?.ToString() ?? string.Empty;
            return new StripeEventEnvelope(stripeEvent.Id, stripeEvent.Type, rawJson);
        }
        catch (StripeException)
        {
            return Error.Validation("VALIDATION_WEBHOOK_SIGNATURE", "Stripe webhook signature verification failed.");
        }
    }
}
