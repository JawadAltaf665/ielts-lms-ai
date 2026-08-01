using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class AiOperationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Enqueue_starts_Queued()
    {
        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "grammar-coach", "some text", Now);

        Assert.Equal(AiOperationStatus.Queued, operation.Status);
        Assert.Equal("some text", operation.InputText);
    }

    [Fact]
    public void MarkProcessing_from_Queued_transitions()
    {
        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "grammar-coach", "text", Now);

        operation.MarkProcessing(Now.AddSeconds(1));

        Assert.Equal(AiOperationStatus.Processing, operation.Status);
    }

    [Fact]
    public void MarkProcessing_is_a_noop_once_already_past_Queued()
    {
        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "grammar-coach", "text", Now);
        operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now.AddSeconds(1));

        operation.MarkProcessing(Now.AddSeconds(2));

        Assert.Equal(AiOperationStatus.Succeeded, operation.Status);
    }

    [Fact]
    public void MarkSucceeded_sets_response_id_and_completed_at()
    {
        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "grammar-coach", "text", Now);
        var responseId = AiResponseId.From(Guid.NewGuid());

        operation.MarkSucceeded(responseId, Now.AddSeconds(2));

        Assert.Equal(AiOperationStatus.Succeeded, operation.Status);
        Assert.Equal(responseId, operation.AiResponseId);
        Assert.Equal(Now.AddSeconds(2), operation.CompletedAtUtc);
    }

    [Fact]
    public void MarkFailed_sets_error_details_and_completed_at()
    {
        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "grammar-coach", "text", Now);

        operation.MarkFailed("DEPENDENCY_AI_UNAVAILABLE", "provider down", Now.AddSeconds(3));

        Assert.Equal(AiOperationStatus.Failed, operation.Status);
        Assert.Equal("DEPENDENCY_AI_UNAVAILABLE", operation.ErrorCode);
        Assert.Equal("provider down", operation.ErrorMessage);
        Assert.Equal(Now.AddSeconds(3), operation.CompletedAtUtc);
    }
}
