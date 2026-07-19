using FluentValidation;

namespace IELTSLms.Application.Tenancy.List;

public sealed class ListTenantsQueryValidator : AbstractValidator<ListTenantsQuery>
{
    public ListTenantsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("VALIDATION_PAGE_INVALID")
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("VALIDATION_PAGE_SIZE_INVALID")
            .WithMessage("Page size must be between 1 and 100.");
    }
}
