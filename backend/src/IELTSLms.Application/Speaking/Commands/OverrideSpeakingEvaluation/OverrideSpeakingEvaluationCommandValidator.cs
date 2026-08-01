using FluentValidation;

namespace IELTSLms.Application.Speaking.Commands.OverrideSpeakingEvaluation;

public sealed class OverrideSpeakingEvaluationCommandValidator : AbstractValidator<OverrideSpeakingEvaluationCommand>
{
    public OverrideSpeakingEvaluationCommandValidator()
    {
        RuleFor(x => x.EvaluationId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.Reason).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.FluencyCoherence).InclusiveBetween(0, 9).WithErrorCode("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.LexicalResource).InclusiveBetween(0, 9).WithErrorCode("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.GrammaticalRangeAccuracy).InclusiveBetween(0, 9).WithErrorCode("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.Pronunciation).InclusiveBetween(0, 9).WithErrorCode("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE");
    }
}
