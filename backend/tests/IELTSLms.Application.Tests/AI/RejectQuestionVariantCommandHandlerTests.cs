using System.Text.Json;
using IELTSLms.Application.AI.QuestionVariants;
using IELTSLms.Application.AI.QuestionVariants.Commands.RejectQuestionVariant;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class RejectQuestionVariantCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static QuestionVariantDraft CreateDraft(Guid tenantId) => QuestionVariantDraft.Create(
        QuestionVariantDraftId.From(Guid.NewGuid()), tenantId, QuestionVariantSourceType.Reading, Guid.NewGuid(),
        JsonSerializer.Serialize(new QuestionVariantContent("MultipleChoice", "Rephrased?", ["a"])), 0,
        AiResponseId.From(Guid.NewGuid()), Now).Value;

    [Fact]
    public async Task Missing_draft_is_rejected()
    {
        var handler = new RejectQuestionVariantCommandHandler(
            new FakeQuestionVariantDraftRepository(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new RejectQuestionVariantCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_QUESTION_VARIANT_DRAFT", result.Error.Code);
    }

    [Fact]
    public async Task Reject_flips_status_and_retains_the_row()
    {
        var drafts = new FakeQuestionVariantDraftRepository();
        var tenantId = Guid.NewGuid();
        var draft = CreateDraft(tenantId);
        drafts.Seed(draft);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RejectQuestionVariantCommandHandler(drafts, unitOfWork, new FakeClock(Now.AddMinutes(1)));

        var result = await handler.Handle(new RejectQuestionVariantCommand(tenantId, draft.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rejected", result.Value.Status);
        Assert.Equal(QuestionVariantStatus.Rejected, draft.Status);
        Assert.False(draft.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Single(drafts.Drafts);
    }

    [Fact]
    public async Task Reject_on_an_already_rejected_draft_fails()
    {
        var drafts = new FakeQuestionVariantDraftRepository();
        var tenantId = Guid.NewGuid();
        var draft = CreateDraft(tenantId);
        draft.Reject(Now);
        drafts.Seed(draft);
        var handler = new RejectQuestionVariantCommandHandler(drafts, new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(1)));

        var result = await handler.Handle(new RejectQuestionVariantCommand(tenantId, draft.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_QUESTION_VARIANT_NOT_PENDING", result.Error.Code);
    }
}
