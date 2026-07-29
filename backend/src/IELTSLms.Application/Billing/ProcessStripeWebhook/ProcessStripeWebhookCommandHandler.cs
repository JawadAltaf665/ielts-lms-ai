using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Billing.ProcessStripeWebhook;

/// <summary>
/// Applies verified Stripe events to local billing state (13 §11). Parses <c>RawJson</c> as generic
/// JSON rather than the Stripe SDK's typed event objects — Application must not depend on the
/// provider SDK (only <c>IELTSLms.Infrastructure</c> may), and the raw event shape is the actual
/// contract Stripe guarantees for webhooks.
/// </summary>
public sealed partial class ProcessStripeWebhookCommandHandler(
    IProcessedWebhookEventRepository processedEvents,
    IBillingCustomerRepository billingCustomers,
    ISubscriptionRepository subscriptions,
    IInvoiceRepository invoices,
    IPaymentRepository payments,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    ILogger<ProcessStripeWebhookCommandHandler> logger)
    : ICommandHandler<ProcessStripeWebhookCommand>
{
    public async Task<Result> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        if (await processedEvents.ExistsAsync(request.EventId, cancellationToken).ConfigureAwait(false))
        {
            WebhookAlreadyProcessed(logger, request.EventId);
            return Result.Success();
        }

        using var document = JsonDocument.Parse(request.RawJson);
        var root = document.RootElement;

        switch (request.EventType)
        {
            case "checkout.session.completed":
                await HandleCheckoutSessionCompletedAsync(root, cancellationToken).ConfigureAwait(false);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpdatedAsync(root, cancellationToken).ConfigureAwait(false);
                break;
            case "invoice.payment_succeeded":
                await HandleInvoicePaymentSucceededAsync(root, cancellationToken).ConfigureAwait(false);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(root, cancellationToken).ConfigureAwait(false);
                break;
            default:
                UnhandledEventType(logger, request.EventType);
                break;
        }

        processedEvents.Add(ProcessedWebhookEvent.Create(request.EventId, clock.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private async Task HandleCheckoutSessionCompletedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeCustomerId = GetString(root, "customer");
        var stripeSubscriptionId = GetString(root, "subscription");
        var localSubscriptionId = GetMetadataGuid(root, "subscription_id");

        if (localSubscriptionId is null || stripeSubscriptionId is null)
        {
            return;
        }

        var subscription = await subscriptions.GetByIdAsync(SubscriptionId.From(localSubscriptionId.Value), cancellationToken)
            .ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "checkout.session.completed", localSubscriptionId.Value.ToString());
            return;
        }

        if (stripeCustomerId is not null)
        {
            var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
            customer?.AttachStripeCustomer(stripeCustomerId, clock.UtcNow);
        }
        // Subscription state itself transitions from the paired customer.subscription.* event, which
        // carries the authoritative status/period fields.
    }

    private async Task HandleSubscriptionUpdatedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = GetString(root, "id");
        if (stripeSubscriptionId is null)
        {
            return;
        }

        var subscription = await ResolveSubscriptionAsync(root, stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "customer.subscription.updated", stripeSubscriptionId);
            return;
        }

        var status = GetString(root, "status");
        var periodStart = GetUnixSeconds(root, "current_period_start") ?? clock.UtcNow;
        var periodEnd = GetUnixSeconds(root, "current_period_end") ?? clock.UtcNow;
        var trialEnd = GetUnixSeconds(root, "trial_end");
        var now = clock.UtcNow;

        switch (status)
        {
            case "trialing" when subscription.Status is SubscriptionStatus.Pending:
                subscription.StartTrial(trialEnd ?? periodEnd, now);
                break;

            case "active" when subscription.Status is SubscriptionStatus.Pending or SubscriptionStatus.Trial or SubscriptionStatus.PastDue:
                subscription.Activate(periodStart, periodEnd, stripeSubscriptionId, now, fromCheckout: subscription.Status is SubscriptionStatus.Pending);
                break;

            case "active" when subscription.Status is SubscriptionStatus.Active && subscription.CurrentPeriodEnd != periodEnd:
                subscription.Renew(periodStart, periodEnd, now);
                break;

            case "past_due" or "unpaid" when subscription.Status is SubscriptionStatus.Active:
                subscription.MarkPastDue(now);
                break;

            case "canceled" when subscription.Status is not (SubscriptionStatus.Cancelled or SubscriptionStatus.Expired):
                subscription.Expire(now);
                break;
        }
    }

    private async Task HandleInvoicePaymentSucceededAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeInvoiceId = GetString(root, "id");
        var stripeSubscriptionId = GetString(root, "subscription");
        if (stripeInvoiceId is null || stripeSubscriptionId is null)
        {
            return;
        }

        var subscription = await subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "invoice.payment_succeeded", stripeSubscriptionId);
            return;
        }

        var amountPaid = GetLong(root, "amount_paid") ?? 0;
        var currency = GetString(root, "currency") ?? "usd";
        var stripePaymentIntentId = GetString(root, "payment_intent");
        var now = clock.UtcNow;

        var invoice = await GetOrCreateInvoiceAsync(subscription, stripeInvoiceId, currency, amountPaid, now, cancellationToken)
            .ConfigureAwait(false);

        if (invoice.Status is InvoiceStatus.Issued)
        {
            invoice.MarkPaid(now);
        }

        var paymentResult = Payment.Create(PaymentId.From(guids.NewGuid()), subscription.TenantId, invoice.Id, amountPaid, currency, now);
        if (paymentResult.IsSuccess)
        {
            var payment = paymentResult.Value;
            payment.MarkSucceeded(now);
            if (stripePaymentIntentId is not null)
            {
                payment.AttachStripePaymentIntent(stripePaymentIntentId, now);
            }

            payments.Add(payment);
        }

        if (subscription.Status is SubscriptionStatus.PastDue)
        {
            subscription.Activate(now, subscription.CurrentPeriodEnd ?? now.AddMonths(1), subscription.StripeSubscriptionId, now);
        }
    }

    private async Task HandleInvoicePaymentFailedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeInvoiceId = GetString(root, "id");
        var stripeSubscriptionId = GetString(root, "subscription");
        if (stripeInvoiceId is null || stripeSubscriptionId is null)
        {
            return;
        }

        var subscription = await subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "invoice.payment_failed", stripeSubscriptionId);
            return;
        }

        var amountDue = GetLong(root, "amount_due") ?? 0;
        var currency = GetString(root, "currency") ?? "usd";
        var now = clock.UtcNow;

        var invoice = await GetOrCreateInvoiceAsync(subscription, stripeInvoiceId, currency, amountDue, now, cancellationToken)
            .ConfigureAwait(false);

        var paymentResult = Payment.Create(PaymentId.From(guids.NewGuid()), subscription.TenantId, invoice.Id, amountDue, currency, now);
        if (paymentResult.IsSuccess)
        {
            var payment = paymentResult.Value;
            payment.MarkFailed("Stripe reported invoice.payment_failed.", now);
            payments.Add(payment);
        }

        if (subscription.Status is SubscriptionStatus.Active)
        {
            subscription.MarkPastDue(now);
        }
    }

    private async Task<Subscription?> ResolveSubscriptionAsync(JsonElement root, string stripeSubscriptionId, CancellationToken cancellationToken)
    {
        var byStripeId = await subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (byStripeId is not null)
        {
            return byStripeId;
        }

        // Out-of-order delivery fallback: the subscription event arrived before checkout.session.completed
        // linked our local Subscription to the Stripe id. Correlate via the Stripe customer instead.
        var stripeCustomerId = GetString(root, "customer");
        if (stripeCustomerId is null)
        {
            return null;
        }

        var customer = await billingCustomers.GetByStripeCustomerIdAsync(stripeCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is null)
        {
            return null;
        }

        return await subscriptions.GetCurrentByTenantAsync(customer.TenantId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Invoice> GetOrCreateInvoiceAsync(
        Subscription subscription, string stripeInvoiceId, string currency, long amountMinorUnits, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await invoices.GetByStripeInvoiceIdAsync(stripeInvoiceId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var sequence = await invoices.CountAllForTenantIncludingDeletedAsync(subscription.TenantId, cancellationToken).ConfigureAwait(false);
        var number = $"INV-{subscription.TenantId:N}-{sequence + 1:D6}";

        var invoice = Invoice.CreateDraft(
            InvoiceId.From(guids.NewGuid()), subscription.TenantId, subscription.Id, subscription.BillingCustomerId, number, currency, now);
        invoice.AttachStripeInvoice(stripeInvoiceId, now);
        invoice.Issue(amountMinorUnits, taxMinorUnits: 0, dueAtUtc: now, now);

        invoices.Add(invoice);
        return invoice;
    }

    private static Guid? GetMetadataGuid(JsonElement root, string key)
    {
        if (root.TryGetProperty("metadata", out var metadata)
            && metadata.TryGetProperty(key, out var value)
            && value.ValueKind == JsonValueKind.String
            && Guid.TryParse(value.GetString(), out var guid))
        {
            return guid;
        }

        return null;
    }

    private static string? GetString(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static long? GetLong(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt64() : null;

    private static DateTimeOffset? GetUnixSeconds(JsonElement root, string property) =>
        GetLong(root, property) is { } seconds ? DateTimeOffset.FromUnixTimeSeconds(seconds) : null;

    [LoggerMessage(EventId = 3100, Level = LogLevel.Information, Message = "Stripe webhook event {EventId} already processed; skipping.")]
    private static partial void WebhookAlreadyProcessed(ILogger logger, string eventId);

    [LoggerMessage(EventId = 3101, Level = LogLevel.Debug, Message = "Unhandled Stripe webhook event type {EventType}.")]
    private static partial void UnhandledEventType(ILogger logger, string eventType);

    [LoggerMessage(EventId = 3102, Level = LogLevel.Warning, Message = "Could not correlate {EventType} to a local subscription (key: {Key}).")]
    private static partial void CorrelationMissed(ILogger logger, string eventType, string key);
}
