using FluentValidation;

namespace IELTSLms.Application.Writing.Commands.OverrideWritingEvaluation;

public sealed class OverrideWritingEvaluationCommandValidator : AbstractValidator<OverrideWritingEvaluationCommand>
{
    public OverrideWritingEvaluationCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");

        RuleFor(x => x.TaskResponse).InclusiveBetween(0m, 9m).WithErrorCode("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.CoherenceCohesion).InclusiveBetween(0m, 9m).WithErrorCode("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.LexicalResource).InclusiveBetween(0m, 9m).WithErrorCode("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE");
        RuleFor(x => x.GrammaticalRangeAccuracy).InclusiveBetween(0m, 9m).WithErrorCode("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE");
    }
}
