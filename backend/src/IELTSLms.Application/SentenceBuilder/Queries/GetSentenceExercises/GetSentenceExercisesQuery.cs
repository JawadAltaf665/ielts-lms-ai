using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercises;

/// <summary>Lists sentence exercises visible to the caller's tenant (global + tenant-authored), filterable by difficulty.</summary>
public sealed record GetSentenceExercisesQuery(Guid TenantId, SentenceDifficulty? Difficulty, int Page, int PageSize)
    : IQuery<PagedList<SentenceExerciseResponse>>;
