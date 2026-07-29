using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Owned value object embedded on <see cref="BillingCustomer"/> — deliberately not its own aggregate,
/// it has no independent lifecycle (ADR-031).
/// </summary>
public sealed class BillingAddress : ValueObject
{
    private BillingAddress()
    {
        // EF Core owned-type materialization.
        Line1 = null!;
        Country = null!;
    }

    private BillingAddress(string line1, string? line2, string? city, string? state, string? postalCode, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string Line1 { get; private set; }

    public string? Line2 { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    public string? PostalCode { get; private set; }

    /// <summary>ISO-3166 alpha-2 country code.</summary>
    public string Country { get; private set; }

    public static BillingAddress Create(
        string line1, string? line2, string? city, string? state, string? postalCode, string country) =>
        new(line1.Trim(), Normalize(line2), Normalize(city), Normalize(state), Normalize(postalCode), country.Trim().ToUpperInvariant());

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
