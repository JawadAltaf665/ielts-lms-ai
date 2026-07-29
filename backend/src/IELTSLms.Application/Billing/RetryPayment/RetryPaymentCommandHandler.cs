using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.RetryPayment;

public sealed class RetryPaymentCommandHandler(
    IPaymentRepository payments, IInvoiceRepository invoices, IPaymentGateway paymentGateway)
    : ICommandHandler<RetryPaymentCommand>
{
    public async Task<Result> Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken).ConfigureAwait(false);
        if (payment is null || payment.TenantId != request.TenantId)
        {
            return Result.Failure(BillingErrors.PaymentNotFound);
        }

        if (payment.Status is not PaymentStatus.Failed)
        {
            return Result.Failure(BillingErrors.PaymentNotRetryable);
        }

        var invoice = await invoices.GetByIdAsync(payment.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice?.StripeInvoiceId is null)
        {
            return Result.Failure(BillingErrors.InvoiceNotFound);
        }

        // Stripe re-attempts the charge and fires invoice.payment_succeeded/failed webhooks with the
        // outcome; local Payment/Subscription state updates there, not synchronously here.
        await paymentGateway.RetryInvoicePaymentAsync(invoice.StripeInvoiceId, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
