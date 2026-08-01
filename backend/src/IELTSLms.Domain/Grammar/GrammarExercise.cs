using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// A drill under a <see cref="GrammarTopic"/> (ERD ENT-016 notes "1..* drills"): a set of questions
/// with a <see cref="GrammarExerciseMode"/>. Mirrors ReadingPassages→ReadingQuestions: no navigation
/// collection, <see cref="GrammarTopicId"/> is an identifier reference only. <see cref="TenantId"/> is
/// nullable for the same global-vs-tenant-authored reason as <see cref="GrammarTopic"/>.
/// </summary>
public sealed class GrammarExercise : AuditableEntity<GrammarExerciseId>
{
    private GrammarExercise(
        GrammarExerciseId id, Guid? tenantId, GrammarTopicId topicId, string title, GrammarExerciseMode mode,
        int order, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        GrammarTopicId = topicId;
        Title = title;
        Mode = mode;
        Order = order;
        CreatedAtUtc = nowUtc;
    }

    private GrammarExercise()
    {
        Title = null!;
    }

    public Guid? TenantId { get; private init; }

    public GrammarTopicId GrammarTopicId { get; private init; }

    public string Title { get; private set; }

    public GrammarExerciseMode Mode { get; private set; }

    public int Order { get; private set; }

    public static Result<GrammarExercise> Create(
        GrammarExerciseId id, Guid? tenantId, GrammarTopicId topicId, string title, GrammarExerciseMode mode,
        int order, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<GrammarExercise>(GrammarDomainErrors.TitleRequired);
        }

        if (order < 0)
        {
            return Result.Failure<GrammarExercise>(Error.Validation(
                "VALIDATION_GRAMMAR_EXERCISE_ORDER", "Order must be zero or greater."));
        }

        return Result.Success(new GrammarExercise(id, tenantId, topicId, title, mode, order, nowUtc));
    }

    public Result Update(string title, GrammarExerciseMode mode, int order, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(GrammarDomainErrors.TitleRequired);
        }

        Title = title;
        Mode = mode;
        Order = order;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
