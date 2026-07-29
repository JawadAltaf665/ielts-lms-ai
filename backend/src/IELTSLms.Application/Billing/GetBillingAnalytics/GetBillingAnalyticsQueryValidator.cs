using FluentValidation;

namespace IELTSLms.Application.Billing.GetBillingAnalytics;

public sealed class GetBillingAnalyticsQueryValidator : AbstractValidator<GetBillingAnalyticsQuery>
{
    public GetBillingAnalyticsQueryValidator()
    {
        RuleFor(x => x.To).GreaterThan(x => x.From).WithErrorCode("VALIDATION_DATE_RANGE");
    }
}
