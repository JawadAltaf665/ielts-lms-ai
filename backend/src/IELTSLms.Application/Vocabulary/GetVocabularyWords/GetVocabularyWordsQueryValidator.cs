using FluentValidation;

namespace IELTSLms.Application.Vocabulary.GetVocabularyWords;

public sealed class GetVocabularyWordsQueryValidator : AbstractValidator<GetVocabularyWordsQuery>
{
    public GetVocabularyWordsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithErrorCode("VALIDATION_PAGE");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_PAGE_SIZE");
    }
}
