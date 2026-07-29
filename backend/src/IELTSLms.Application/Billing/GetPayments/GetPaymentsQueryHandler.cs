using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetPayments;

public sealed class GetPaymentsQueryHandler(IPaymentRepository payments) : IQueryHandler<GetPaymentsQuery, PagedList<PaymentSummaryResponse>>
{
    public async Task<Result<PagedList<PaymentSummaryResponse>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var items = await payments.ListByTenantAsync(request.TenantId, skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var total = await payments.CountByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        var page = PagedList<PaymentSummaryResponse>.Create(
            items.Select(ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }

    internal static PaymentSummaryResponse ToResponse(Payment payment) => new(
        payment.Id.Value, payment.InvoiceId.Value, payment.Status.ToString(), payment.AmountMinorUnits, payment.Currency,
        payment.FailureReason, payment.PaidAtUtc, payment.CreatedAtUtc);
}
