using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;

public sealed class CreateSentenceExerciseCommandHandler(
    ISentenceExerciseRepository exercises, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateSentenceExerciseCommand, SentenceExerciseResponse>
{
    public async Task<Result<SentenceExerciseResponse>> Handle(CreateSentenceExerciseCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        // Exercises created through the tenant-facing API are always tenant-owned; global-catalog
        // exercises (TenantId null) are seeded content, not authored here — mirrors CreateGrammarTopic.
        var createResult = SentenceExercise.Create(
            SentenceExerciseId.From(guids.NewGuid()), request.TenantId, request.Prompt,
            JsonSerializer.Serialize(request.TargetPattern), request.Difficulty, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<SentenceExerciseResponse>(createResult.Error);
        }

        var exercise = createResult.Value;
        exercises.Add(exercise);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(exercise));
    }

    internal static SentenceExerciseResponse ToResponse(SentenceExercise exercise) => new(
        exercise.Id.Value, exercise.TenantId, exercise.Prompt,
        JsonSerializer.Deserialize<string[]>(exercise.TargetPatternJson) ?? [], exercise.Difficulty.ToString(), exercise.IsGlobal);
}
