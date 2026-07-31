using FluentValidation;

namespace IELTSLms.Application.Listening.CreateListeningAudio;

public sealed class CreateListeningAudioCommandValidator : AbstractValidator<CreateListeningAudioCommand>
{
    public CreateListeningAudioCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("VALIDATION_REQUIRED")
            .MaximumLength(200).WithErrorCode("VALIDATION_TOO_LONG");

        RuleFor(x => x.DurationSeconds).GreaterThan(0).WithErrorCode("VALIDATION_LISTENING_DURATION_INVALID");
        RuleFor(x => x.FileName).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.ContentType).NotEmpty().WithErrorCode("VALIDATION_REQUIRED");
        RuleFor(x => x.SizeBytes).GreaterThan(0).WithErrorCode("VALIDATION_FILE_SIZE_INVALID");
    }
}
