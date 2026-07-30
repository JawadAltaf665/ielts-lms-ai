using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class AiResponseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var result = AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), Guid.NewGuid(), AiProviderId.From(Guid.NewGuid()),
            AiPromptId.From(Guid.NewGuid()), 1, "AiOperation", Guid.NewGuid(), "hash123", "claude-3-5-haiku",
            10, 20, 150, 0, "USD", AiResponseStatus.Succeeded, """{"corrections":[]}""", "msg_1", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(AiResponseStatus.Succeeded, result.Value.Status);
        Assert.Equal(10, result.Value.PromptTokens);
    }

    [Fact]
    public void Create_with_blank_input_hash_fails()
    {
        var result = AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), Guid.NewGuid(), AiProviderId.From(Guid.NewGuid()), null, 1,
            "AiOperation", Guid.NewGuid(), "  ", "claude-3-5-haiku", 0, 0, 0, 0, "USD", AiResponseStatus.Failed,
            null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_RESPONSE_INPUT_HASH_REQUIRED", result.Error.Code);
    }
}
