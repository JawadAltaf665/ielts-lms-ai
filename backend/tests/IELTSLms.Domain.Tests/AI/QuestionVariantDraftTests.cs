using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class QuestionVariantDraftTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static QuestionVariantDraft CreateDraft() => QuestionVariantDraft.Create(
        QuestionVariantDraftId.From(Guid.NewGuid()), Guid.NewGuid(), QuestionVariantSourceType.Reading, Guid.NewGuid(),
        """{"type":"MultipleChoice","prompt":"...","answerKey":["A"]}""", 1, AiResponseId.From(Guid.NewGuid()), Now).Value;

    [Fact]
    public void Create_with_valid_content_starts_pending()
    {
        var draft = CreateDraft();

        Assert.Equal(QuestionVariantStatus.Pending, draft.Status);
    }

    [Fact]
    public void Create_with_blank_draft_content_fails()
    {
        var result = QuestionVariantDraft.Create(
            QuestionVariantDraftId.From(Guid.NewGuid()), Guid.NewGuid(), QuestionVariantSourceType.Reading, Guid.NewGuid(),
            "   ", 0, AiResponseId.From(Guid.NewGuid()), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_QUESTION_VARIANT_DRAFT_CONTENT_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Approve_on_a_pending_draft_succeeds()
    {
        var draft = CreateDraft();

        var result = draft.Approve(Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(QuestionVariantStatus.Approved, draft.Status);
    }

    [Fact]
    public void Reject_on_a_pending_draft_succeeds_and_retains_the_row()
    {
        var draft = CreateDraft();

        var result = draft.Reject(Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(QuestionVariantStatus.Rejected, draft.Status);
        Assert.False(draft.IsDeleted);
    }

    [Fact]
    public void Approve_on_an_already_approved_draft_fails()
    {
        var draft = CreateDraft();
        draft.Approve(Now);

        var result = draft.Approve(Now.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_QUESTION_VARIANT_NOT_PENDING", result.Error.Code);
    }

    [Fact]
    public void Reject_on_an_already_rejected_draft_fails()
    {
        var draft = CreateDraft();
        draft.Reject(Now);

        var result = draft.Reject(Now.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_QUESTION_VARIANT_NOT_PENDING", result.Error.Code);
    }
}
