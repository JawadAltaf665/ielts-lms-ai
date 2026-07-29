using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetInvoices;

public sealed class GetInvoicesQueryHandler(IInvoiceRepository invoices) : IQueryHandler<GetInvoicesQuery, PagedList<InvoiceSummaryResponse>>
{
    public async Task<Result<PagedList<InvoiceSummaryResponse>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        InvoiceStatus? status = Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var parsed) ? parsed : null;
        var skip = (request.Page - 1) * request.PageSize;

        var items = await invoices.SearchAsync(request.TenantId, status, skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var total = await invoices.CountSearchAsync(request.TenantId, status, cancellationToken).ConfigureAwait(false);

        var page = PagedList<InvoiceSummaryResponse>.Create(
            items.Select(ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }

    internal static InvoiceSummaryResponse ToResponse(Invoice invoice) => new(
        invoice.Id.Value, invoice.Number, invoice.Status.ToString(), invoice.Currency, invoice.TotalMinorUnits,
        invoice.IssuedAtUtc, invoice.PaidAtUtc, invoice.HostedInvoiceUrl);
}
