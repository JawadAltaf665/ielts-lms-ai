namespace IELTSLms.Infrastructure.Billing;

/// <summary>
/// Stripe API configuration (13 §10). Left blank in `appsettings.json` for local development, same
/// placeholder convention as <c>JwtOptions</c>' PEM keys — a real secret key/webhook signing secret
/// MUST be configured in any deployed environment (never committed). Unlike <c>JwtOptions</c>' required
/// fields, these are intentionally NOT <c>[Required]</c>/<c>ValidateOnStart</c>: Billing is one module
/// among many, and a blank Stripe key must not prevent the whole API from starting in local dev or CI.
/// Absence is instead caught at first actual use (see <see cref="StripePaymentGateway"/>).
/// </summary>
public sealed class StripeOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Stripe";

    /// <summary>Stripe secret API key (server-side only, never exposed to clients).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Stripe publishable key, safe to expose to the client for Checkout/Elements.</summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>Signing secret used to verify inbound webhook signatures (13 §11).</summary>
    public string WebhookSigningSecret { get; set; } = string.Empty;
}
