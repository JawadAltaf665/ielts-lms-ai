using FluentValidation;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;

public sealed class CreateSpeakingAttemptCommandValidator : AbstractValidator<CreateSpeakingAttemptCommand>
{
    public CreateSpeakingAttemptCommandValidator()
    {
        RuleFor(x => x.SpeakingTaskId).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.DurationSeconds).GreaterThan(0).WithErrorCode("VALIDATION_SPEAKING_DURATION_INVALID");
        RuleFor(x => x.FileName).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.ContentType).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.SizeBytes).GreaterThan(0).WithErrorCode("VALIDATION_FILE_SIZE_INVALID");
    }
}
