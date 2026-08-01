using FluentValidation;

namespace IELTSLms.Application.Reading.GetReadingPassages;

public sealed class GetReadingPassagesQueryValidator : AbstractValidator<GetReadingPassagesQuery>
{
    public GetReadingPassagesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("VALIDATION_PAGE");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_PAGE_SIZE");
    }
}
