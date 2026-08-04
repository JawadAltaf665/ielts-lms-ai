using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercise;

public sealed record GetSentenceExerciseQuery(Guid TenantId, Guid ExerciseId) : IQuery<SentenceExerciseResponse>;
