using IELTSLms.Application.Billing.RefundPayment;
using IELTSLms.Application.Tests.Billing.Fakes;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing;

public sealed class RefundPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Payment Payment, FakePaymentRepository Payments, FakeRefundRepository Refunds, FakePaymentGateway Gateway, FakeUnitOfWork UnitOfWork)
        BuildScenario(Guid tenantId, long amount)
    {
        var paymentResult = Payment.Create(PaymentId.From(Guid.NewGuid()), tenantId, InvoiceId.From(Guid.NewGuid()), amount, "usd", Now);
        var payment = paymentResult.Value;
        payment.MarkSucceeded(Now);
        payment.AttachStripePaymentIntent("pi_123", Now);

        var payments = new FakePaymentRepository();
        payments.Seed(payment);

        return (payment, payments, new FakeRefundRepository(), new FakePaymentGateway(), new FakeUnitOfWork());
    }

    [Fact]
    public async Task Full_refund_succeeds_and_marks_payment_refunded()
    {
        var tenantId = Guid.NewGuid();
        var (payment, payments, refunds, gateway, unitOfWork) = BuildScenario(tenantId, 1000);
        var handler = new RefundPaymentCommandHandler(
            payments, refunds, new FakeBillingCustomerRepository(), gateway, new FakeEmailSender(), unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new RefundPaymentCommand(tenantId, payment.Id.Value, 1000, "Customer request"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(1, gateway.RefundCallCount);
    }

    [Fact]
    public async Task Partial_refund_succeeds_and_leaves_payment_succeeded()
    {
        var tenantId = Guid.NewGuid();
        var (payment, payments, refunds, gateway, unitOfWork) = BuildScenario(tenantId, 1000);
        var handler = new RefundPaymentCommandHandler(
            payments, refunds, new FakeBillingCustomerRepository(), gateway, new FakeEmailSender(), unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new RefundPaymentCommand(tenantId, payment.Id.Value, 400, "Partial goodwill credit"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }

    [Fact]
    public async Task Refund_exceeding_remaining_balance_fails()
    {
        var tenantId = Guid.NewGuid();
        var (payment, payments, refunds, gateway, unitOfWork) = BuildScenario(tenantId, 1000);
        var handler = new RefundPaymentCommandHandler(
            payments, refunds, new FakeBillingCustomerRepository(), gateway, new FakeEmailSender(), unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        // First refund uses up 700 of the 1000, leaving 300 remaining.
        await handler.Handle(new RefundPaymentCommand(tenantId, payment.Id.Value, 700, "First partial"), CancellationToken.None);

        var result = await handler.Handle(new RefundPaymentCommand(tenantId, payment.Id.Value, 400, "Second partial"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_REFUND_EXCEEDS_PAYMENT", result.Error.Code);
    }

    [Fact]
    public async Task Refund_of_non_succeeded_payment_fails()
    {
        var tenantId = Guid.NewGuid();
        var paymentResult = Payment.Create(PaymentId.From(Guid.NewGuid()), tenantId, InvoiceId.From(Guid.NewGuid()), 1000, "usd", Now);
        var payment = paymentResult.Value;
        var payments = new FakePaymentRepository();
        payments.Seed(payment);

        var handler = new RefundPaymentCommandHandler(
            payments, new FakeRefundRepository(), new FakeBillingCustomerRepository(), new FakePaymentGateway(), new FakeEmailSender(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new RefundPaymentCommand(tenantId, payment.Id.Value, 500, "Too early"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_PAYMENT_NOT_REFUNDABLE", result.Error.Code);
    }
}
