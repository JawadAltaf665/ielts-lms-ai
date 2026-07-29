using FluentValidation;

namespace IELTSLms.Application.Billing.CreateBillingPortalSession;

public sealed class CreateBillingPortalSessionCommandValidator : AbstractValidator<CreateBillingPortalSessionCommand>
{
    public CreateBillingPortalSessionCommandValidator()
    {
        RuleFor(x => x.ReturnUrl)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .Must(BeAbsoluteHttpUrl).WithErrorCode("VALIDATION_URL_INVALID");
    }

    private static bool BeAbsoluteHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var parsed) && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
}
