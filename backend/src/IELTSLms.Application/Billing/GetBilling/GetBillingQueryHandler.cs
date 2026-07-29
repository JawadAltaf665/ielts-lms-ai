using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetBilling;

public sealed class GetBillingQueryHandler(
    IBillingCustomerRepository billingCustomers, IPaymentMethodRepository paymentMethods, IInvoiceRepository invoices)
    : IQueryHandler<GetBillingQuery, BillingSummaryResponse>
{
    private const int RecentInvoiceCount = 10;

    public async Task<Result<BillingSummaryResponse>> Handle(GetBillingQuery request, CancellationToken cancellationToken)
    {
        var customer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        var methods = customer is null
            ? []
            : await paymentMethods.ListByCustomerAsync(customer.Id, cancellationToken).ConfigureAwait(false);

        var recentInvoices = await invoices
            .ListByTenantAsync(request.TenantId, skip: 0, take: RecentInvoiceCount, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new BillingSummaryResponse(
            customer?.Id.Value,
            customer?.Email,
            customer?.Name,
            methods.Select(m => new PaymentMethodResponse(m.Id.Value, m.Brand, m.Last4, m.ExpMonth, m.ExpYear, m.IsDefault)).ToArray(),
            recentInvoices.Select(i => new InvoiceSummaryResponse(
                i.Id.Value, i.Number, i.Status.ToString(), i.Currency, i.TotalMinorUnits, i.IssuedAtUtc, i.PaidAtUtc)).ToArray()));
    }
}
