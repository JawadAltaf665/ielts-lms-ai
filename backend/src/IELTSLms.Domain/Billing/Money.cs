using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Monetary amount in minor units (e.g. cents) with an ISO-4217 currency code (ADR-031).</summary>
public sealed class Money : ValueObject
{
    private Money(long amountMinorUnits, string currency)
    {
        AmountMinorUnits = amountMinorUnits;
        Currency = currency;
    }

    public long AmountMinorUnits { get; }

    public string Currency { get; }

    public static Result<Money> Create(long amountMinorUnits, string? currency)
    {
        if (amountMinorUnits < 0)
        {
            return Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            return Error.Validation("VALIDATION_CURRENCY_INVALID", "Currency must be a 3-letter ISO-4217 code.");
        }

        return new Money(amountMinorUnits, currency.Trim().ToUpperInvariant());
    }

    /// <summary>Rehydrates a value known to be valid (e.g. from the database) without re-validating.</summary>
    public static Money FromTrusted(long amountMinorUnits, string currency) => new(amountMinorUnits, currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AmountMinorUnits;
        yield return Currency;
    }

    public override string ToString() => $"{AmountMinorUnits} {Currency}";
}
