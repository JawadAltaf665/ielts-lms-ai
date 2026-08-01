using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class AiPromptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var result = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, AiProviderId.From(Guid.NewGuid()), "grammar-coach", 1, "default",
            "system template", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PromptVersion);
        Assert.True(result.Value.IsActive);
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

        prompt.Deactivate(Now.AddDays(1));

        Assert.False(prompt.IsActive);
        Assert.Equal(Now.AddDays(1), prompt.UpdatedAtUtc);
    }
}
