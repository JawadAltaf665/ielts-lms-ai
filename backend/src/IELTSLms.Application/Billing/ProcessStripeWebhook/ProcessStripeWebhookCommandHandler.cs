using System.Text.Json;
using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Billing.ProcessStripeWebhook;

/// <summary>
/// Applies verified Stripe events to local billing state (13 §11, ADR-032). Parses <c>RawJson</c> as
/// generic JSON rather than the Stripe SDK's typed event objects — Application must not depend on the
/// provider SDK (only <c>IELTSLms.Infrastructure</c> may), and the raw event shape is the actual
/// contract Stripe guarantees for webhooks. A handler that throws is dead-lettered (
/// <see cref="FailedWebhookEvent"/>) rather than left to bubble, so the endpoint still fast-acks and
/// ops can inspect/replay via <c>DeadLetterWebhookRetryJob</c>.
/// </summary>
public sealed partial class ProcessStripeWebhookCommandHandler(
    IProcessedWebhookEventRepository processedEvents,
    IFailedWebhookEventRepository failedEvents,
    IBillingCustomerRepository billingCustomers,
    ISubscriptionRepository subscriptions,
    IInvoiceRepository invoices,
    IPaymentRepository payments,
    IEmailSender emailSender,
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

        try
        {
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
                case "customer.subscription.deleted":
                    await HandleSubscriptionDeletedAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "invoice.created":
                    await HandleInvoiceCreatedAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "invoice.finalized":
                    await HandleInvoiceFinalizedAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "invoice.payment_succeeded":
                    await HandleInvoicePaymentSucceededAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "invoice.payment_failed":
                    await HandleInvoicePaymentFailedAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "customer.created":
                case "customer.updated":
                    await HandleCustomerSyncedAsync(root, cancellationToken).ConfigureAwait(false);
                    break;
                case "payment_intent.succeeded":
                case "payment_intent.failed":
                    // No-op by design: invoice.payment_succeeded/failed are authoritative for
                    // subscription billing (Stripe's own recommendation) and already drive Payment/
                    // Subscription state. These events exist mainly for one-off PaymentIntent flows
                    // this module doesn't use; still recorded in the idempotency ledger below.
                    break;
                default:
                    UnhandledEventType(logger, request.EventType);
                    break;
            }

            processedEvents.Add(ProcessedWebhookEvent.Create(request.EventId, clock.UtcNow));

            // This attempt succeeded — if it was a dead-letter retry, the failure record is resolved.
            var previouslyFailed = await failedEvents.GetByIdAsync(request.EventId, cancellationToken).ConfigureAwait(false);
            if (previouslyFailed is not null)
            {
                failedEvents.Remove(previouslyFailed);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await DeadLetterAsync(request, ex, cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }

    private async Task DeadLetterAsync(ProcessStripeWebhookCommand request, Exception exception, CancellationToken cancellationToken)
    {
        WebhookProcessingFailed(logger, request.EventId, request.EventType, exception);

        // Discard whatever partial aggregate state the failed handler left tracked — only the
        // dead-letter marker itself should be persisted.
        unitOfWork.DiscardChanges();

        var existing = await failedEvents.GetByIdAsync(request.EventId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            existing.RecordRetryAttempt(exception.Message, clock.UtcNow);
        }
        else
        {
            failedEvents.Add(FailedWebhookEvent.Create(request.EventId, request.EventType, request.RawJson, exception.Message, clock.UtcNow));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
        var wasPending = subscription.Status is SubscriptionStatus.Pending;

        switch (status)
        {
            case "trialing" when subscription.Status is SubscriptionStatus.Pending:
                subscription.StartTrial(trialEnd ?? periodEnd, now);
                break;

            case "active" when subscription.Status is SubscriptionStatus.Pending or SubscriptionStatus.Trial or SubscriptionStatus.PastDue:
                subscription.Activate(periodStart, periodEnd, stripeSubscriptionId, now, fromCheckout: wasPending);
                await NotifySubscriptionActivatedAsync(subscription, cancellationToken).ConfigureAwait(false);
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

    private async Task HandleSubscriptionDeletedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = GetString(root, "id");
        if (stripeSubscriptionId is null)
        {
            return;
        }

        var subscription = await subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "customer.subscription.deleted", stripeSubscriptionId);
            return;
        }

        if (subscription.Status is not (SubscriptionStatus.Cancelled or SubscriptionStatus.Expired))
        {
            subscription.Expire(clock.UtcNow);
        }
    }

    private async Task HandleInvoiceCreatedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeInvoiceId = GetString(root, "id");
        var stripeSubscriptionId = GetString(root, "subscription");
        if (stripeInvoiceId is null || stripeSubscriptionId is null)
        {
            return;
        }

        if (await invoices.GetByStripeInvoiceIdAsync(stripeInvoiceId, cancellationToken).ConfigureAwait(false) is not null)
        {
            return;
        }

        var subscription = await subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            CorrelationMissed(logger, "invoice.created", stripeSubscriptionId);
            return;
        }

        var currency = GetString(root, "currency") ?? "usd";
        var now = clock.UtcNow;
        var number = await NextInvoiceNumberAsync(subscription.TenantId, cancellationToken).ConfigureAwait(false);

        var invoice = Invoice.CreateDraft(
            InvoiceId.From(guids.NewGuid()), subscription.TenantId, subscription.Id, subscription.BillingCustomerId, number, currency, now);
        invoice.AttachStripeInvoice(stripeInvoiceId, now);
        invoices.Add(invoice);
    }

    private async Task HandleInvoiceFinalizedAsync(JsonElement root, CancellationToken cancellationToken)
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
            CorrelationMissed(logger, "invoice.finalized", stripeSubscriptionId);
            return;
        }

        var subtotal = GetLong(root, "subtotal") ?? 0;
        var tax = GetLong(root, "tax") ?? 0;
        var total = GetLong(root, "total") ?? subtotal + tax;
        var currency = GetString(root, "currency") ?? "usd";
        var hostedInvoiceUrl = GetString(root, "hosted_invoice_url");
        var now = clock.UtcNow;

        var invoice = await GetOrCreateInvoiceAsync(subscription, stripeInvoiceId, currency, total, now, cancellationToken).ConfigureAwait(false);
        if (invoice.Status is InvoiceStatus.Draft)
        {
            invoice.Issue(subtotal, tax, dueAtUtc: now.AddDays(7), now);
        }

        invoice.SetHostedInvoiceUrl(hostedInvoiceUrl, now);

        var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is not null)
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    "Your invoice is ready",
                    $"<p>Invoice {invoice.Number} for {total} {currency.ToUpperInvariant()} is ready.</p>",
                    $"Invoice {invoice.Number} for {total} {currency.ToUpperInvariant()} is ready."),
                cancellationToken).ConfigureAwait(false);
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

        var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is not null)
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    "Payment successful",
                    $"<p>Your payment of {amountPaid} {currency.ToUpperInvariant()} was successful.</p>",
                    $"Your payment of {amountPaid} {currency.ToUpperInvariant()} was successful."),
                cancellationToken).ConfigureAwait(false);
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

        var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is not null)
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    "Payment failed",
                    $"<p>Your payment of {amountDue} {currency.ToUpperInvariant()} failed. Please update your payment method.</p>",
                    $"Your payment of {amountDue} {currency.ToUpperInvariant()} failed. Please update your payment method."),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task HandleCustomerSyncedAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var stripeCustomerId = GetString(root, "id");
        var email = GetString(root, "email");
        if (stripeCustomerId is null)
        {
            return;
        }

        var customer = await billingCustomers.GetByStripeCustomerIdAsync(stripeCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is null || email is null)
        {
            return;
        }

        var name = GetString(root, "name");
        customer.UpdateProfile(email, name, phone: null, address: null, clock.UtcNow);
    }

    private async Task NotifySubscriptionActivatedAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer is null)
        {
            return;
        }

        await emailSender.SendAsync(
            new EmailMessage(
                customer.Email,
                "Your subscription is active",
                "<p>Your subscription is now active. Welcome aboard!</p>",
                "Your subscription is now active. Welcome aboard!"),
            cancellationToken).ConfigureAwait(false);
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

        var number = await NextInvoiceNumberAsync(subscription.TenantId, cancellationToken).ConfigureAwait(false);

        var invoice = Invoice.CreateDraft(
            InvoiceId.From(guids.NewGuid()), subscription.TenantId, subscription.Id, subscription.BillingCustomerId, number, currency, now);
        invoice.AttachStripeInvoice(stripeInvoiceId, now);
        invoice.Issue(amountMinorUnits, taxMinorUnits: 0, dueAtUtc: now, now);

        invoices.Add(invoice);
        return invoice;
    }

    private async Task<string> NextInvoiceNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var sequence = await invoices.CountAllForTenantIncludingDeletedAsync(tenantId, cancellationToken).ConfigureAwait(false);
        return $"INV-{tenantId:N}-{sequence + 1:D6}";
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

    [LoggerMessage(EventId = 3103, Level = LogLevel.Error, Message = "Stripe webhook event {EventId} ({EventType}) failed processing; dead-lettered.")]
    private static partial void WebhookProcessingFailed(ILogger logger, string eventId, string eventType, Exception exception);
}
