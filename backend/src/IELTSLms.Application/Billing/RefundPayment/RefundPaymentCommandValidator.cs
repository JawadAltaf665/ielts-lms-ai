using FluentValidation;

namespace IELTSLms.Application.Billing.RefundPayment;

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.AmountMinorUnits).GreaterThan(0).WithErrorCode("VALIDATION_AMOUNT_NEGATIVE");
        RuleFor(x => x.Reason).NotEmpty().WithErrorCode("VALIDATION_REQUIRED").MaximumLength(500).WithErrorCode("VALIDATION_TOO_LONG");
    }
}
