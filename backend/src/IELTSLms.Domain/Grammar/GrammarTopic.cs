using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// A grammar unit (ERD ENT-016, DDD §13.7). <see cref="TenantId"/> is nullable: <see langword="null"/>
/// for global-catalog topics authored once and visible to every tenant (read-only to tenants),
/// non-null for a tenant's own authored topic — mirrors <c>Role</c>'s system-vs-custom split. Because
/// of the nullable tenant this does not implement <see cref="ITenantScoped"/> (which requires a
/// non-nullable <c>Guid</c>), so it is exempt from the automatic tenant query filter; handlers must
/// filter explicitly (global OR caller's tenant).
/// </summary>
public sealed class GrammarTopic : AuditableEntity<GrammarTopicId>
{
    private GrammarTopic(
        GrammarTopicId id, Guid? tenantId, string title, string description, GrammarTrack track,
        GrammarDifficulty difficulty, Guid? categoryId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Description = description;
        Track = track;
        Difficulty = difficulty;
        CategoryId = categoryId;
        CreatedAtUtc = nowUtc;
    }

    private GrammarTopic()
    {
        Title = null!;
        Description = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog topic.</summary>
    public Guid? TenantId { get; private init; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public GrammarTrack Track { get; private set; }

    public GrammarDifficulty Difficulty { get; private set; }

    public Guid? CategoryId { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<GrammarTopic> Create(
        GrammarTopicId id, Guid? tenantId, string title, string description, GrammarTrack track,
        GrammarDifficulty difficulty, Guid? categoryId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<GrammarTopic>(GrammarDomainErrors.TitleRequired);
        }

        return Result.Success(new GrammarTopic(id, tenantId, title, description, track, difficulty, categoryId, nowUtc));
    }

    /// <summary>Updates the topic's content. Global topics are read-only to tenants; callers must
    /// already have verified ownership before calling this (BUSINESS_GRAMMAR_TOPIC_READ_ONLY).</summary>
    public Result Update(
        string title, string description, GrammarTrack track, GrammarDifficulty difficulty, Guid? categoryId,
        DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(GrammarDomainErrors.TitleRequired);
        }

        Title = title;
        Description = description;
        Track = track;
        Difficulty = difficulty;
        CategoryId = categoryId;
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

internal static class GrammarDomainErrors
{
    public static readonly Error TitleRequired = Error.Validation(
        "VALIDATION_GRAMMAR_TITLE_REQUIRED", "Title is required.");
}
