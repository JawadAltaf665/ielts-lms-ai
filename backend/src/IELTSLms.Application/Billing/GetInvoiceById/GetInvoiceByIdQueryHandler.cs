using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetInvoices;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(IInvoiceRepository invoices) : IQueryHandler<GetInvoiceByIdQuery, InvoiceSummaryResponse>
{
    public async Task<Result<InvoiceSummaryResponse>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetByIdAsync(InvoiceId.From(request.InvoiceId), cancellationToken).ConfigureAwait(false);
        if (invoice is null || invoice.TenantId != request.TenantId)
        {
            return Result.Failure<InvoiceSummaryResponse>(BillingErrors.InvoiceNotFound);
        }

        return Result.Success(GetInvoicesQueryHandler.ToResponse(invoice));
    }
}
