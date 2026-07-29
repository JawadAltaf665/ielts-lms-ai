using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Line item of an <see cref="Invoice"/> (ERD ENT-039O). Immutable once the parent invoice is issued.</summary>
public sealed class InvoiceLine : AuditableEntity<InvoiceLineId>
{
    private InvoiceLine(
        InvoiceLineId id, InvoiceId invoiceId, PriceId? priceId, string description, int quantity,
        long unitAmountMinorUnits, DateTimeOffset nowUtc)
        : base(id)
    {
        InvoiceId = invoiceId;
        PriceId = priceId;
        Description = description;
        Quantity = quantity;
        UnitAmountMinorUnits = unitAmountMinorUnits;
        CreatedAtUtc = nowUtc;
    }

    private InvoiceLine()
    {
        Description = null!;
    }

    public InvoiceId InvoiceId { get; private set; }

    public PriceId? PriceId { get; private set; }

    public string Description { get; private set; }

    public int Quantity { get; private set; }

    public long UnitAmountMinorUnits { get; private set; }

    public long AmountMinorUnits => UnitAmountMinorUnits * Quantity;

    public static Result<InvoiceLine> Create(
        InvoiceLineId id, InvoiceId invoiceId, PriceId? priceId, string description, int quantity,
        long unitAmountMinorUnits, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Invoice line description is required.");
        }

        if (quantity < 1)
        {
            return Error.Validation("VALIDATION_QUANTITY", "Quantity must be at least 1.");
        }

        if (unitAmountMinorUnits < 0)
        {
            return Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Unit amount cannot be negative.");
        }

        return new InvoiceLine(id, invoiceId, priceId, description.Trim(), quantity, unitAmountMinorUnits, nowUtc);
    }
}
