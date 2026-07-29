using FluentValidation;

namespace IELTSLms.Application.Billing.CreateSubscription;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .EmailAddress().WithErrorCode("VALIDATION_EMAIL_INVALID")
            .MaximumLength(320).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PlanId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
    }
}
