using System.Text;
using IELTSLms.Abstractions.Billing;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Billing.ProcessStripeWebhook;
using MediatR;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Receives inbound payment-provider webhooks (13 §11). Signature verification requires the exact
/// raw request body bytes Stripe signed, so this endpoint reads the body directly rather than relying
/// on JSON model binding — the first place in the codebase that needs raw-body access.
/// </summary>
internal static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/webhooks/stripe", HandleStripeWebhookAsync)
            .AllowAnonymous()
            .WithTags("Webhooks")
            .WithName("StripeWebhook")
            .WithSummary("Receives and processes Stripe webhook events (signature-verified, not JWT-authenticated).");

        return builder;
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpContext httpContext, IStripeWebhookVerifier verifier, ISender sender, CancellationToken cancellationToken)
    {
        string payload;
        using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        if (!httpContext.Request.Headers.TryGetValue("Stripe-Signature", out var signature) || signature.Count == 0)
        {
            return Results.BadRequest();
        }

        var verifyResult = verifier.ConstructEvent(payload, signature.ToString());
        if (verifyResult.IsFailure)
        {
            return verifyResult.ToProblem(httpContext);
        }

        var envelope = verifyResult.Value;
        var result = await sender.Send(
            new ProcessStripeWebhookCommand(envelope.EventId, envelope.EventType, envelope.RawJson), cancellationToken);

        // Ack once verified and processed; Stripe retries the delivery on any non-2xx response (13 §11).
        return result.IsSuccess ? Results.Ok() : result.ToProblem(httpContext);
    }
}
