using FluentValidation;

namespace IELTSLms.Application.Vocabulary.GradeVocabularyReview;

public sealed class GradeVocabularyReviewCommandValidator : AbstractValidator<GradeVocabularyReviewCommand>
{
    public GradeVocabularyReviewCommandValidator()
    {
        RuleFor(x => x.Grade).InclusiveBetween(0, 5).WithErrorCode("VALIDATION_VOCABULARY_GRADE_RANGE");
    }
}
