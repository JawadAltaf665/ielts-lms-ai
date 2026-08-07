using System.Text.Json;
using IELTSLms.Application.AI.QuestionVariants.Commands.GenerateQuestionVariants;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using FakeListeningAudioRepository = IELTSLms.Application.Tests.Listening.Fakes.FakeListeningAudioRepository;
using FakeListeningQuestionRepository = IELTSLms.Application.Tests.Listening.Fakes.FakeListeningQuestionRepository;
using FakeReadingPassageRepository = IELTSLms.Application.Tests.Reading.Fakes.FakeReadingPassageRepository;
using FakeReadingQuestionRepository = IELTSLms.Application.Tests.Reading.Fakes.FakeReadingQuestionRepository;

namespace IELTSLms.Application.Tests.AI;

public sealed class GenerateQuestionVariantsCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    private static GenerateQuestionVariantsCommandHandler CreateHandler(
        FakeReadingPassageRepository passages, FakeReadingQuestionRepository readingQuestions,
        FakeListeningAudioRepository audios, FakeListeningQuestionRepository listeningQuestions,
        FakeAiOperationRepository operations, FakeUnitOfWork unitOfWork, FakeBackgroundJobService backgroundJobs) =>
        new(passages, readingQuestions, audios, listeningQuestions, operations, unitOfWork, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task Reading_passage_not_found_is_rejected_without_enqueueing()
    {
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            new FakeReadingPassageRepository(), new FakeReadingQuestionRepository(), new FakeListeningAudioRepository(),
            new FakeListeningQuestionRepository(), operations, new FakeUnitOfWork(), backgroundJobs);

        var result = await handler.Handle(
            new GenerateQuestionVariantsCommand(
                Guid.NewGuid(), Guid.NewGuid(), QuestionVariantSourceType.Reading, Guid.NewGuid(), Guid.NewGuid(), 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_PASSAGE", result.Error.Code);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Global_reading_passage_is_read_only()
    {
        var passages = new FakeReadingPassageRepository();
        var tenantId = Guid.NewGuid();
        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), null, "Global Passage", "text", ReadingTrack.Academic,
            ReadingDifficulty.Intermediate, null, 1800, Now).Value;
        passages.Seed(passage);
        var handler = CreateHandler(
            passages, new FakeReadingQuestionRepository(), new FakeListeningAudioRepository(), new FakeListeningQuestionRepository(),
            new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeBackgroundJobService());

        var result = await handler.Handle(
            new GenerateQuestionVariantsCommand(
                tenantId, Guid.NewGuid(), QuestionVariantSourceType.Reading, passage.Id.Value, Guid.NewGuid(), 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_READING_PASSAGE_READ_ONLY", result.Error.Code);
    }

    [Fact]
    public async Task Source_question_not_on_the_passage_is_rejected()
    {
        var passages = new FakeReadingPassageRepository();
        var tenantId = Guid.NewGuid();
        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "Ocean Currents", "text", ReadingTrack.Academic,
            ReadingDifficulty.Intermediate, null, 1800, Now).Value;
        passages.Seed(passage);
        var handler = CreateHandler(
            passages, new FakeReadingQuestionRepository(), new FakeListeningAudioRepository(), new FakeListeningQuestionRepository(),
            new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeBackgroundJobService());

        var result = await handler.Handle(
            new GenerateQuestionVariantsCommand(
                tenantId, Guid.NewGuid(), QuestionVariantSourceType.Reading, passage.Id.Value, Guid.NewGuid(), 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_QUESTION_VARIANT_SOURCE", result.Error.Code);
    }

    [Fact]
    public async Task Reading_success_composes_grounded_text_and_enqueues_the_job()
    {
        var passages = new FakeReadingPassageRepository();
        var questions = new FakeReadingQuestionRepository();
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var backgroundJobs = new FakeBackgroundJobService();
        var tenantId = Guid.NewGuid();

        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "Ocean Currents", "Oceans move heat around the globe.",
            ReadingTrack.Academic, ReadingDifficulty.Intermediate, null, 1800, Now).Value;
        passages.Seed(passage);
        var question = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passage.Id, ReadingQuestionType.MultipleChoice,
            "What drives ocean currents?", Key("heat"), 0, Now).Value;
        questions.Seed(question);

        var handler = CreateHandler(
            passages, questions, new FakeListeningAudioRepository(), new FakeListeningQuestionRepository(), operations, unitOfWork,
            backgroundJobs);

        var result = await handler.Handle(
            new GenerateQuestionVariantsCommand(
                tenantId, Guid.NewGuid(), QuestionVariantSourceType.Reading, passage.Id.Value, question.Id.Value, 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
        var operation = Assert.Single(operations.Operations);
        Assert.Equal("reading-question-variants", operation.Kind);
        Assert.Contains("Oceans move heat around the globe.", operation.InputText);
        Assert.Contains("What drives ocean currents?", operation.InputText);
        Assert.Contains("+1", operation.InputText);
    }

    [Fact]
    public async Task Listening_success_composes_grounded_text_and_enqueues_the_job()
    {
        var audios = new FakeListeningAudioRepository();
        var questions = new FakeListeningQuestionRepository();
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var backgroundJobs = new FakeBackgroundJobService();
        var tenantId = Guid.NewGuid();

        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Campus Tour", ReadingTrack.General, FileAssetId.From(Guid.NewGuid()),
            null, 600, "The library closes at nine.", Now).Value;
        audios.Seed(audio);
        var question = ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audio.Id, ReadingQuestionType.MultipleChoice,
            "When does the library close?", Key("nine"), 0, Now).Value;
        questions.Seed(question);

        var handler = CreateHandler(
            new FakeReadingPassageRepository(), new FakeReadingQuestionRepository(), audios, questions, operations, unitOfWork,
            backgroundJobs);

        var result = await handler.Handle(
            new GenerateQuestionVariantsCommand(
                tenantId, Guid.NewGuid(), QuestionVariantSourceType.Listening, audio.Id.Value, question.Id.Value, -1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var operation = Assert.Single(operations.Operations);
        Assert.Equal("listening-question-variants", operation.Kind);
        Assert.Contains("The library closes at nine.", operation.InputText);
        Assert.Contains("-1", operation.InputText);
    }
}
