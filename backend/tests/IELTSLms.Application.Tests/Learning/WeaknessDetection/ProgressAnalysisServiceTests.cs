using System.Text.Json;
using System.Text.Json.Serialization;
using IELTSLms.Abstractions.AI;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Learning.WeaknessDetection;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;
using FakeProgressTrackingRepository = IELTSLms.Application.Tests.Learning.Fakes.FakeProgressTrackingRepository;
using FakeGuidProvider = IELTSLms.Application.Tests.Learning.Fakes.FakeGuidProvider;

namespace IELTSLms.Application.Tests.Learning.WeaknessDetection;

/// <summary>
/// The sprint's own required Tier-1 fixture: "seed attempts/evaluations across at least Grammar,
/// Vocabulary, Reading, and Writing for a test learner, then assert the weakness profile correctly
/// identifies the seeded weak skill(s)." Exercised at the Application layer with Fakes — same posture
/// <c>CrossModuleLearnerJourneyTests</c> documents for this codebase (no live-database Integration.Tests
/// exist here). The actual weakness *scoring* is the AI Gateway's job (Agentic AI Architecture §12.2),
/// which cannot run for real in an offline unit test — so this proves the wiring: the seeded cross-skill
/// history reaches the gateway request, and whatever the gateway returns is what ends up persisted in
/// the queryable <see cref="Domain.Learning.WeaknessProfile"/> (the part this codebase's own code is
/// responsible for getting right).
/// </summary>
public sealed class ProgressAnalysisServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerOptions.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Below_the_minimum_data_threshold_computes_nothing_and_never_calls_the_gateway()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var grammar = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Grammar, Now);
        grammar.RecordActivity(Now);
        progressTracking.Seed(grammar);

        var gateway = new FakeAiGateway();
        var service = CreateService(progressTracking, gateway, out _);

        var result = await service.ComputeAsync(tenantId, userId, Now, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(0, gateway.InvokeCallCount);
    }

    [Fact]
    public async Task A_learner_weak_in_writing_but_strong_elsewhere_gets_that_reflected_in_the_persisted_profile()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Cross-skill history: strong Grammar/Vocabulary/Reading, weak Writing (low criterion scores) —
        // the seeded pattern the persisted profile must reflect.
        var grammarAttempts = new FakeGrammarAttemptRepository();
        grammarAttempts.Seed(GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, GrammarExerciseId.From(Guid.NewGuid()), Guid.NewGuid(),
            "[]", 9, 10, Now));

        var vocabularyProgress = new FakeVocabularyProgressRepository();
        var strongVocab = VocabularyProgress.Create(
            VocabularyProgressId.From(Guid.NewGuid()), tenantId, userId, VocabularyWordId.From(Guid.NewGuid()), Now);
        strongVocab.Grade(quality: 5, Now.AddMinutes(1));
        vocabularyProgress.Seed(strongVocab);

        var readingAttempts = new FakeReadingAttemptRepository();
        var strongReading = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, ReadingPassageId.From(Guid.NewGuid()), Now.AddMinutes(2), 1800);
        strongReading.Submit("[]", 38, 40, 8.5m, Now.AddMinutes(10));
        readingAttempts.Seed(strongReading);

        var writingSubmissions = new FakeWritingSubmissionRepository();
        var writingEvaluations = new FakeWritingEvaluationRepository();
        var weakWriting = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "a thin essay", Now.AddMinutes(15)).Value;
        weakWriting.Submit(wordCount: 120, Now.AddMinutes(20));
        var evaluation = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, weakWriting.Id, taskResponse: 4.0m, coherenceCohesion: 4.0m,
            lexicalResource: 4.0m, grammaticalRangeAccuracy: 4.0m, confidence: 0.9m, feedbackText: "needs work",
            lowConfidenceThreshold: 0.7m, similarityScore: null, highSimilarityThreshold: 0.6m,
            aiResponseId: null, aiProviderId: null, Now.AddMinutes(25)).Value;
        weakWriting.AttachEvaluation(evaluation.Id, evaluation.EstimatedBand, Now.AddMinutes(25));
        writingSubmissions.Seed(weakWriting);
        writingEvaluations.Seed(evaluation);

        var progressTracking = new FakeProgressTrackingRepository();
        SeedProgress(progressTracking, tenantId, userId, LearningSkill.Grammar, 3);
        SeedProgress(progressTracking, tenantId, userId, LearningSkill.Vocabulary, 3);
        SeedProgress(progressTracking, tenantId, userId, LearningSkill.Reading, 3);
        SeedProgress(progressTracking, tenantId, userId, LearningSkill.Writing, 3);

        // The AI Gateway is what actually scores weakness (Agentic AI Architecture §12.2) — configured
        // here to return the same conclusion a real analysis of this history would (Writing is weakest),
        // so the test proves the plumbing carries that conclusion through to persistence correctly.
        var gateway = new FakeAiGateway
        {
            NextResult = Result.Success(new AiGatewayInvocationResult(
                """
                {"scores":[
                  {"skill":"Grammar","weaknessScore":10,"note":"strong"},
                  {"skill":"Vocabulary","weaknessScore":10,"note":"strong"},
                  {"skill":"Reading","weaknessScore":5,"note":"strong"},
                  {"skill":"Listening","weaknessScore":50,"note":"no data"},
                  {"skill":"Writing","weaknessScore":90,"note":"weak"},
                  {"skill":"Speaking","weaknessScore":50,"note":"no data"}
                ]}
                """,
                "claude-3-5-haiku", 100, 50, 200, 0, "USD", "msg_1", FromCache: false)),
        };

        var service = CreateService(
            progressTracking, gateway, out var weaknessProfiles, writingSubmissions, writingEvaluations, grammarAttempts,
            readingAttempts, vocabularyProgress);

        var result = await service.ComputeAsync(tenantId, userId, Now.AddHours(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, gateway.InvokeCallCount);

        var persisted = Assert.Single(weaknessProfiles.Profiles);
        var scores = JsonSerializer.Deserialize<List<SkillWeaknessScoreDto>>(persisted.SkillScoresJson, JsonOptions)!;
        var weakest = scores.MaxBy(s => s.Score)!;
        Assert.Equal(LearningSkill.Writing, weakest.Skill);
        Assert.True(scores.First(s => s.Skill == LearningSkill.Grammar).Score < weakest.Score);
    }

    private static void SeedProgress(FakeProgressTrackingRepository repo, Guid tenantId, Guid userId, LearningSkill skill, int activityCount)
    {
        var row = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, skill, Now);
        for (var i = 0; i < activityCount; i++)
        {
            row.RecordActivity(Now);
        }

        repo.Seed(row);
    }

    private static ProgressAnalysisService CreateService(
        FakeProgressTrackingRepository progressTracking, FakeAiGateway gateway, out FakeWeaknessProfileRepository weaknessProfiles,
        FakeWritingSubmissionRepository? writingSubmissions = null, FakeWritingEvaluationRepository? writingEvaluations = null,
        FakeGrammarAttemptRepository? grammarAttempts = null, FakeReadingAttemptRepository? readingAttempts = null,
        FakeVocabularyProgressRepository? vocabularyProgress = null)
    {
        weaknessProfiles = new FakeWeaknessProfileRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);
        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, provider.Id, "progress-analysis", 1, "default", "system prompt", Now).Value;
        prompt.Activate(Now);
        prompts.Seed(prompt);

        return new ProgressAnalysisService(
            progressTracking, weaknessProfiles, providers, prompts, new FakeAiResponseRepository(),
            writingSubmissions ?? new FakeWritingSubmissionRepository(), writingEvaluations ?? new FakeWritingEvaluationRepository(),
            new FakeSpeakingAttemptRepository(), new FakeSpeakingEvaluationRepository(),
            readingAttempts ?? new FakeReadingAttemptRepository(), new FakeListeningAttemptRepository(),
            grammarAttempts ?? new FakeGrammarAttemptRepository(), vocabularyProgress ?? new FakeVocabularyProgressRepository(),
            gateway, new FakeGuidProvider());
    }
}
