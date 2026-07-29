using FluentValidation;

namespace IELTSLms.Application.Billing.GetFinancialSummary;

public sealed class GetFinancialSummaryQueryValidator : AbstractValidator<GetFinancialSummaryQuery>
{
    public GetFinancialSummaryQueryValidator()
    {
        RuleFor(x => x.To).GreaterThan(x => x.From).WithErrorCode("VALIDATION_DATE_RANGE");
    }
}
