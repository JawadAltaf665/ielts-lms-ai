using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class AiPromptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_succeeds_as_an_inactive_draft()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, AiProviderId.From(Guid.NewGuid()), "grammar-coach", 1, "default",
            "system template", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PromptVersion);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public void Create_with_blank_feature_fails()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "  ", 1, "default", "system template", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_PROMPT_FEATURE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_non_positive_version_fails()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 0, "default", "system template", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_PROMPT_VERSION_INVALID", result.Error.Code);
    }

    [Fact]
    public void Create_with_blank_system_template_fails()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 1, "default", "   ", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_PROMPT_SYSTEM_TEMPLATE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_blank_scope_defaults_to_default()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 1, "  ", "system template", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("default", result.Value.Scope);
    }

    [Fact]
    public void Deactivate_clears_IsActive_and_stamps_updated_at()
    {
        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 1, "default", "system template", Now).Value;
        prompt.Activate(Now);

        prompt.Deactivate(Now.AddDays(1));

        Assert.False(prompt.IsActive);
        Assert.Equal(Now.AddDays(1), prompt.UpdatedAtUtc);
    }

    [Fact]
    public void Activate_sets_IsActive_and_stamps_updated_at()
    {
        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 1, "default", "system template", Now).Value;

        prompt.Activate(Now.AddDays(1));

        Assert.True(prompt.IsActive);
        Assert.Equal(Now.AddDays(1), prompt.UpdatedAtUtc);
    }

    [Fact]
    public void SoftDelete_sets_IsDeleted_and_stamps_deleted_and_updated_at()
    {
        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "grammar-coach", 1, "default", "system template", Now).Value;

        prompt.SoftDelete(Now.AddDays(1));

        Assert.True(prompt.IsDeleted);
        Assert.Equal(Now.AddDays(1), prompt.DeletedAtUtc);
        Assert.Equal(Now.AddDays(1), prompt.UpdatedAtUtc);
    }

    [Fact]
    public void There_is_no_way_to_mutate_content_after_creation_other_than_creating_a_new_version()
    {
        // Immutability enforcement (AI Architecture §5): SystemTemplate/Feature/Scope/PromptVersion are
        // `private init`-only on the domain type — the only public mutators are IsActive/IsDeleted state
        // transitions (Activate/Deactivate/SoftDelete). An "edit" of a used version is not just rejected
        // at runtime, there is no method surface through which the Application layer could even attempt one.
        var contentMutators = typeof(AiPrompt).GetMethods()
            .Where(m => m.DeclaringType == typeof(AiPrompt) && m.IsPublic && !m.IsStatic && !m.IsSpecialName)
            .Select(m => m.Name)
            .ToArray();

        Assert.Equal(["Activate", "Deactivate", "SoftDelete"], contentMutators.OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }
}
