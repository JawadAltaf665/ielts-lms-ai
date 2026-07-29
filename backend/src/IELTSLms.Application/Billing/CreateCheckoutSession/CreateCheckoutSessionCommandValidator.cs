using FluentValidation;

namespace IELTSLms.Application.Billing.CreateCheckoutSession;

public sealed class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .EmailAddress().WithErrorCode("VALIDATION_EMAIL_INVALID")
            .MaximumLength(320).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PlanId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.PriceId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.SuccessUrl)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .Must(BeAbsoluteHttpUrl).WithErrorCode("VALIDATION_URL_INVALID");

        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .Must(BeAbsoluteHttpUrl).WithErrorCode("VALIDATION_URL_INVALID");
    }

    private static bool BeAbsoluteHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var parsed) && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
}
