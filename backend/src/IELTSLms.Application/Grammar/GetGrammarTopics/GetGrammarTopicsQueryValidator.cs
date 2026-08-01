using FluentValidation;

namespace IELTSLms.Application.Grammar.GetGrammarTopics;

public sealed class GetGrammarTopicsQueryValidator : AbstractValidator<GetGrammarTopicsQuery>
{
    public GetGrammarTopicsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("VALIDATION_PAGE");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_PAGE_SIZE");
    }
}
