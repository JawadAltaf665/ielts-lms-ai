using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// A non-cash correction applied against an issued <see cref="Invoice"/> (ERD ENT-039Q, ADR-032).
/// Invoices are immutable once issued (13 §8.1); credit notes are how corrections are recorded.
/// </summary>
public sealed class CreditNote : TenantAuditableEntity<CreditNoteId>
{
    private CreditNote(
        CreditNoteId id, Guid tenantId, InvoiceId invoiceId, long amountMinorUnits, string reason, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        AmountMinorUnits = amountMinorUnits;
        Reason = reason;
        Status = CreditNoteStatus.Issued;
        CreatedAtUtc = nowUtc;
    }

    private CreditNote()
    {
        Reason = null!;
    }

    public InvoiceId InvoiceId { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Reason { get; private set; }

    public CreditNoteStatus Status { get; private set; }

    public static Result<CreditNote> Issue(
        CreditNoteId id, Guid tenantId, Invoice invoice, long amountMinorUnits, string reason, DateTimeOffset nowUtc)
    {
        if (amountMinorUnits <= 0)
        {
            return Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Credit note amount must be positive.");
        }

        if (amountMinorUnits > invoice.TotalMinorUnits)
        {
            return Error.Validation("VALIDATION_CREDIT_NOTE_EXCEEDS_INVOICE", "Credit note amount cannot exceed the invoice total.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Credit note reason is required.");
        }

        return new CreditNote(id, tenantId, invoice.Id, amountMinorUnits, reason.Trim(), nowUtc);
    }

    public Result Void(DateTimeOffset nowUtc)
    {
        if (Status is CreditNoteStatus.Void)
        {
            return Result.Success();
        }

        Status = CreditNoteStatus.Void;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
