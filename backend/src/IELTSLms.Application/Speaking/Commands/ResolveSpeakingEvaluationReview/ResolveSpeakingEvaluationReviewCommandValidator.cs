using FluentValidation;

namespace IELTSLms.Application.Speaking.Commands.ResolveSpeakingEvaluationReview;

public sealed class ResolveSpeakingEvaluationReviewCommandValidator : AbstractValidator<ResolveSpeakingEvaluationReviewCommand>
{
    public ResolveSpeakingEvaluationReviewCommandValidator()
    {
        RuleFor(x => x.EvaluationId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.Decision).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
    }
}
