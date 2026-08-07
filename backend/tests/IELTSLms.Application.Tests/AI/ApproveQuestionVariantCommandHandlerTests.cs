using System.Text.Json;
using IELTSLms.Application.AI.QuestionVariants;
using IELTSLms.Application.AI.QuestionVariants.Commands.ApproveQuestionVariant;
using IELTSLms.Application.Listening.AddListeningQuestion;
using IELTSLms.Application.Reading.AddReadingQuestion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using FakeReadingQuestionRepository = IELTSLms.Application.Tests.Reading.Fakes.FakeReadingQuestionRepository;

namespace IELTSLms.Application.Tests.AI;

public sealed class ApproveQuestionVariantCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Fact]
    public async Task Missing_draft_is_rejected()
    {
        var handler = new ApproveQuestionVariantCommandHandler(
            new FakeQuestionVariantDraftRepository(), new FakeReadingQuestionRepository(),
            new Application.Tests.Listening.Fakes.FakeListeningQuestionRepository(), new FakeSender(), new FakeUnitOfWork(),
            new FakeClock(Now));

        var result = await handler.Handle(new ApproveQuestionVariantCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_QUESTION_VARIANT_DRAFT", result.Error.Code);
    }

    [Fact]
    public async Task Approve_promotes_via_AddReadingQuestionCommand_with_the_next_sequence_and_flips_status()
    {
        var drafts = new FakeQuestionVariantDraftRepository();
        var readingQuestions = new FakeReadingQuestionRepository();
        var tenantId = Guid.NewGuid();
        var passageId = ReadingPassageId.From(Guid.NewGuid());

        var existingQuestion = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passageId, ReadingQuestionType.MultipleChoice, "Q1",
            Key("a"), 0, Now).Value;
        readingQuestions.Seed(existingQuestion);

        var sourceQuestion = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passageId, ReadingQuestionType.MultipleChoice,
            "What drives ocean currents?", Key("heat"), 1, Now).Value;
        readingQuestions.Seed(sourceQuestion);

        var content = new QuestionVariantContent("MultipleChoice", "What primarily drives ocean currents?", ["heat"]);
        var draft = QuestionVariantDraft.Create(
            QuestionVariantDraftId.From(Guid.NewGuid()), tenantId, QuestionVariantSourceType.Reading, sourceQuestion.Id.Value,
            JsonSerializer.Serialize(content), 1, AiResponseId.From(Guid.NewGuid()), Now).Value;
        drafts.Seed(draft);

        var sender = new FakeSender
        {
            NextResponse = Result.Success(new ReadingQuestionResponse(Guid.NewGuid(), passageId.Value, "MultipleChoice", content.Prompt, 2)),
        };
        var handler = new ApproveQuestionVariantCommandHandler(
            drafts, readingQuestions, new Application.Tests.Listening.Fakes.FakeListeningQuestionRepository(), sender,
            new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(1)));

        var result = await handler.Handle(new ApproveQuestionVariantCommand(tenantId, draft.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value.Status);
        Assert.Equal(QuestionVariantStatus.Approved, draft.Status);
        var promoted = Assert.IsType<AddReadingQuestionCommand>(sender.LastRequest);
        Assert.Equal(passageId.Value, promoted.PassageId);
        Assert.Equal(2, promoted.Sequence);
        Assert.Equal(content.Prompt, promoted.Prompt);
    }

    [Fact]
    public async Task Approve_on_an_already_approved_draft_fails_without_re_promoting()
    {
        var drafts = new FakeQuestionVariantDraftRepository();
        var readingQuestions = new FakeReadingQuestionRepository();
        var tenantId = Guid.NewGuid();
        var passageId = ReadingPassageId.From(Guid.NewGuid());
        var sourceQuestion = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passageId, ReadingQuestionType.MultipleChoice, "Q",
            Key("a"), 0, Now).Value;
        readingQuestions.Seed(sourceQuestion);

        var content = new QuestionVariantContent("MultipleChoice", "Rephrased?", ["a"]);
        var draft = QuestionVariantDraft.Create(
            QuestionVariantDraftId.From(Guid.NewGuid()), tenantId, QuestionVariantSourceType.Reading, sourceQuestion.Id.Value,
            JsonSerializer.Serialize(content), 0, AiResponseId.From(Guid.NewGuid()), Now).Value;
        draft.Approve(Now);
        drafts.Seed(draft);

        var sender = new FakeSender
        {
            NextResponse = Result.Success(new ReadingQuestionResponse(Guid.NewGuid(), passageId.Value, "MultipleChoice", "Rephrased?", 1)),
        };
        var handler = new ApproveQuestionVariantCommandHandler(
            drafts, readingQuestions, new Application.Tests.Listening.Fakes.FakeListeningQuestionRepository(), sender,
            new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(1)));

        var result = await handler.Handle(new ApproveQuestionVariantCommand(tenantId, draft.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_QUESTION_VARIANT_NOT_PENDING", result.Error.Code);
        Assert.Null(sender.LastRequest);
    }
}
