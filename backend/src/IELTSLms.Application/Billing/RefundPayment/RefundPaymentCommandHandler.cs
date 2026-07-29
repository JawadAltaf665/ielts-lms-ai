using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.RefundPayment;

public sealed class RefundPaymentCommandHandler(
    IPaymentRepository payments, IRefundRepository refunds, IBillingCustomerRepository billingCustomers,
    IPaymentGateway paymentGateway, IEmailSender emailSender, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<RefundPaymentCommand, RefundResponse>
{
    public async Task<Result<RefundResponse>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken).ConfigureAwait(false);
        if (payment is null || payment.TenantId != request.TenantId)
        {
            return Result.Failure<RefundResponse>(BillingErrors.PaymentNotFound);
        }

        if (payment.Status is not PaymentStatus.Succeeded)
        {
            return Result.Failure<RefundResponse>(BillingErrors.PaymentNotRefundable);
        }

        var existingRefunds = await refunds.ListByPaymentAsync(payment.Id, cancellationToken).ConfigureAwait(false);
        var alreadyRefunded = existingRefunds.Where(r => r.Status is not RefundStatus.Failed).Sum(r => r.AmountMinorUnits);
        if (alreadyRefunded + request.AmountMinorUnits > payment.AmountMinorUnits)
        {
            return Result.Failure<RefundResponse>(BillingErrors.RefundExceedsPayment);
        }

        var now = clock.UtcNow;
        var createResult = Refund.Create(RefundId.From(guids.NewGuid()), request.TenantId, payment.Id, request.AmountMinorUnits, request.Reason, now);
        if (createResult.IsFailure)
        {
            return Result.Failure<RefundResponse>(createResult.Error);
        }

        var refund = createResult.Value;

        if (payment.StripePaymentIntentId is not null)
        {
            var stripeRefundId = await paymentGateway
                .CreateRefundAsync(payment.StripePaymentIntentId, request.AmountMinorUnits, cancellationToken)
                .ConfigureAwait(false);
            refund.AttachStripeRefund(stripeRefundId, now);
        }

        refund.MarkSucceeded(now);

        if (alreadyRefunded + request.AmountMinorUnits >= payment.AmountMinorUnits)
        {
            payment.Refund(now);
        }

        refunds.Add(refund);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var customer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (customer is not null)
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    "Your refund has been processed",
                    $"<p>A refund of {request.AmountMinorUnits} {payment.Currency} has been processed for your payment.</p>",
                    $"A refund of {request.AmountMinorUnits} {payment.Currency} has been processed for your payment."),
                cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(new RefundResponse(refund.Id.Value, refund.AmountMinorUnits, refund.Status.ToString()));
    }
}
