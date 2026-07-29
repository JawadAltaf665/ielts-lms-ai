using FluentValidation;

namespace IELTSLms.Application.Billing.GetPayments;

public sealed class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("VALIDATION_PAGE");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_PAGE_SIZE");
    }
}
