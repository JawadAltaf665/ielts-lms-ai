using FluentValidation;

namespace IELTSLms.Application.AI.Prompts.ListPromptTemplates;

public sealed class ListPromptTemplatesQueryValidator : AbstractValidator<ListPromptTemplatesQuery>
{
    public ListPromptTemplatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("VALIDATION_PAGE");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_PAGE_SIZE");
    }
}
