using IELTSLms.Application.AI.GetAiOperationStatus;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class GetAiOperationStatusQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Queued_operation_reports_status_with_no_result()
    {
        var operations = new FakeAiOperationRepository();
        var responses = new FakeAiResponseRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var operation = AiOperation.Enqueue(AiOperationId.From(Guid.NewGuid()), tenantId, userId, "grammar-coach", "text", Now);
        operations.Seed(operation);

        var handler = new GetAiOperationStatusQueryHandler(operations, responses);
        var result = await handler.Handle(new GetAiOperationStatusQuery(tenantId, userId, operation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Null(result.Value.Result);
    }

    [Fact]
    public async Task Succeeded_operation_embeds_the_parsed_result()
    {
        var operations = new FakeAiOperationRepository();
        var responses = new FakeAiResponseRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var operation = AiOperation.Enqueue(AiOperationId.From(Guid.NewGuid()), tenantId, userId, "grammar-coach", "text", Now);

        var response = IELTSLms.Domain.AI.AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), tenantId, AiProviderId.From(Guid.NewGuid()), null, 1, "AiOperation",
            operation.Id.Value, "hash", "claude-3-5-haiku", 5, 5, 100, 0, "USD", AiResponseStatus.Succeeded,
            """{"corrections":[{"span":"He go","errorType":"subject-verb-agreement","suggestion":"He goes","explanation":"needs -s"}]}""",
            "msg_1", Now).Value;
        responses.Seed(response);
        operation.MarkSucceeded(response.Id, Now);
        operations.Seed(operation);

        var handler = new GetAiOperationStatusQueryHandler(operations, responses);
        var result = await handler.Handle(new GetAiOperationStatusQuery(tenantId, userId, operation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("succeeded", result.Value.Status);
        Assert.NotNull(result.Value.Result);
        Assert.Single(result.Value.Result!.Value.GetProperty("corrections").EnumerateArray());
    }

    [Fact]
    public async Task Operation_owned_by_a_different_user_is_hidden_as_not_found()
    {
        var operations = new FakeAiOperationRepository();
        var responses = new FakeAiResponseRepository();
        var tenantId = Guid.NewGuid();
        var operation = AiOperation.Enqueue(AiOperationId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), "grammar-coach", "text", Now);
        operations.Seed(operation);

        var handler = new GetAiOperationStatusQueryHandler(operations, responses);
        var result = await handler.Handle(
            new GetAiOperationStatusQuery(tenantId, Guid.NewGuid(), operation.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_OPERATION", result.Error.Code);
    }

    [Fact]
    public async Task Missing_operation_is_not_found()
    {
        var handler = new GetAiOperationStatusQueryHandler(new FakeAiOperationRepository(), new FakeAiResponseRepository());

        var result = await handler.Handle(
            new GetAiOperationStatusQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_OPERATION", result.Error.Code);
    }
}
