using System.Text.Json;
using IELTSLms.Application.AI.QuestionVariants;
using IELTSLms.Application.AI.QuestionVariants.Queries.GetQuestionVariant;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class GetQuestionVariantQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Missing_draft_is_not_found()
    {
        var handler = new GetQuestionVariantQueryHandler(new FakeQuestionVariantDraftRepository());

        var result = await handler.Handle(new GetQuestionVariantQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_QUESTION_VARIANT_DRAFT", result.Error.Code);
    }

    [Fact]
    public async Task Existing_draft_returns_its_parsed_content()
    {
        var drafts = new FakeQuestionVariantDraftRepository();
        var tenantId = Guid.NewGuid();
        var sourceQuestionId = Guid.NewGuid();
        var content = new QuestionVariantContent("MultipleChoice", "What primarily drives ocean currents?", ["heat"]);
        var draft = QuestionVariantDraft.Create(
            QuestionVariantDraftId.From(Guid.NewGuid()), tenantId, QuestionVariantSourceType.Reading, sourceQuestionId,
            JsonSerializer.Serialize(content), 1, AiResponseId.From(Guid.NewGuid()), Now).Value;
        drafts.Seed(draft);
        var handler = new GetQuestionVariantQueryHandler(drafts);

        var result = await handler.Handle(new GetQuestionVariantQuery(tenantId, draft.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
        Assert.Equal("Reading", result.Value.SourceType);
        Assert.Equal(sourceQuestionId, result.Value.SourceQuestionId);
        Assert.Equal(content.Prompt, result.Value.Content.Prompt);
        Assert.Equal(1, result.Value.DifficultyDelta);
    }
}
