using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Sellable catalog item, mirrors a Stripe Product (ERD ENT-039A).</summary>
public sealed class Product : AuditableEntity<ProductId>
{
    private Product(ProductId id, string internalCode, string name, string? description, DateTimeOffset nowUtc)
        : base(id)
    {
        InternalCode = internalCode;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private Product()
    {
        InternalCode = null!;
        Name = null!;
    }

    public const int NameMaxLength = 256;
    public const int CodeMaxLength = 64;

    public string InternalCode { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public string? StripeProductId { get; private set; }

    public static Product Create(ProductId id, string internalCode, string name, string? description, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(internalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Product(id, internalCode.Trim(), name.Trim(), description, nowUtc);
    }

    public void AttachStripeProduct(string stripeProductId, DateTimeOffset nowUtc)
    {
        StripeProductId = stripeProductId;
        UpdatedAtUtc = nowUtc;
    }

    public Result Archive(DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Reactivate(DateTimeOffset nowUtc)
    {
        if (IsActive)
        {
            return Result.Success();
        }

        IsActive = true;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
