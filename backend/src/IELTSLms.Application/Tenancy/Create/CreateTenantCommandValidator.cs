using FluentValidation;
using IELTSLms.Domain.Tenancy;

namespace IELTSLms.Application.Tenancy.Create;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Slug is required.")
            .MaximumLength(TenantSlug.MaxLength).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Code is required.")
            .MaximumLength(TenantCode.MaxLength).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED").WithMessage("Name is required.")
            .MaximumLength(Tenant.NameMaxLength).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.PrimaryDomain)
            .MaximumLength(Tenant.DomainMaxLength).WithErrorCode("VALIDATION_TOO_LONG")
            .When(x => x.PrimaryDomain is not null);

        RuleFor(x => x.ContactEmail)
            .MaximumLength(Tenant.ContactEmailMaxLength).WithErrorCode("VALIDATION_TOO_LONG")
            .When(x => x.ContactEmail is not null);

        RuleFor(x => x.DefaultLocale)
            .MaximumLength(Tenant.LocaleMaxLength).WithErrorCode("VALIDATION_TOO_LONG")
            .When(x => x.DefaultLocale is not null);

        RuleFor(x => x.DefaultTimeZone)
            .MaximumLength(Tenant.LocaleMaxLength).WithErrorCode("VALIDATION_TOO_LONG")
            .When(x => x.DefaultTimeZone is not null);
    }
}
