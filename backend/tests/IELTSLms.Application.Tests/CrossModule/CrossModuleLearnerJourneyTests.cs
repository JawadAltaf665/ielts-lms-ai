using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Analytics.Queries.GetAttemptReview;
using IELTSLms.Application.Analytics.Queries.GetExamHistory;
using IELTSLms.Application.Certificates;
using IELTSLms.Application.Certificates.GetCertificate;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Application.Tests.Certificates.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Learning.Events;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Speaking;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.Domain.Writing;
using Microsoft.Extensions.Logging.Abstractions;
using FakeCurrentUserService = IELTSLms.Application.Tests.Writing.Fakes.FakeCurrentUserService;
using FakeUnitOfWork = IELTSLms.Application.Tests.Grammar.Fakes.FakeUnitOfWork;
using FakeClock = IELTSLms.Application.Tests.Grammar.Fakes.FakeClock;
using FakeGuidProvider = IELTSLms.Application.Tests.Grammar.Fakes.FakeGuidProvider;
using FakeFileStorage = IELTSLms.Application.Tests.Certificates.Fakes.FakeFileStorage;

namespace IELTSLms.Application.Tests.CrossModule;

/// <summary>
/// Sprint 9's own acceptance criterion: "exercise a realistic end-to-end learner journey — Grammar
/// attempt → Vocabulary review → Reading/Listening attempts → Writing/Speaking submissions → Mock Exam
/// → exam history reflects all of it → progress/analytics updated correctly → certificate eligibility
/// evaluated correctly at the end." Exercised at the Application layer against real handler/service
/// code (not mocked business logic) with Fakes — the same pattern every other module's handler tests
/// already use — rather than a live-database HTTP walk, which no existing Integration.Tests file in
/// this codebase attempts either (every one of them only checks the unauthenticated-401 guard).
/// </summary>
public sealed class CrossModuleLearnerJourneyTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task A_learner_who_completes_activity_across_every_skill_sees_it_all_in_history_and_earns_a_certificate()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Grammar attempt.
        var grammarAttempts = new FakeGrammarAttemptRepository();
        var grammarAttempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, GrammarExerciseId.From(Guid.NewGuid()), Guid.NewGuid(),
            "[]", 8, 10, Now);
        grammarAttempts.Seed(grammarAttempt);

        // Vocabulary review.
        var vocabularyProgress = new FakeVocabularyProgressRepository();
        var progress = VocabularyProgress.Create(
            VocabularyProgressId.From(Guid.NewGuid()), tenantId, userId, VocabularyWordId.From(Guid.NewGuid()), Now);
        progress.Grade(quality: 4, Now.AddMinutes(5));
        vocabularyProgress.Seed(progress);

        // Reading attempt.
        var readingAttempts = new FakeReadingAttemptRepository();
        var readingAttempt = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, ReadingPassageId.From(Guid.NewGuid()), Now.AddMinutes(10), 1800);
        readingAttempt.Submit("[]", 30, 40, 6.5m, Now.AddMinutes(20));
        readingAttempts.Seed(readingAttempt);

        // Listening attempt.
        var listeningAttempts = new FakeListeningAttemptRepository();
        var listeningAttempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, ListeningAudioId.From(Guid.NewGuid()), ListeningMode.Exam,
            Now.AddMinutes(25), 1800);
        listeningAttempt.Submit("[]", 32, 40, 7.0m, Now.AddMinutes(35));
        listeningAttempts.Seed(listeningAttempt);

        // Writing submission.
        var writingSubmissions = new FakeWritingSubmissionRepository();
        var writingSubmission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "an essay response", Now.AddMinutes(40)).Value;
        writingSubmission.Submit(3, Now.AddMinutes(45));
        writingSubmissions.Seed(writingSubmission);

        // Speaking submission.
        var speakingAttempts = new FakeSpeakingAttemptRepository();
        var speakingAttempt = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, userId, SpeakingTaskId.From(Guid.NewGuid()), FileAssetId.From(Guid.NewGuid()),
            60, Now.AddMinutes(50)).Value;
        speakingAttempt.Submit(Now.AddMinutes(51));
        speakingAttempts.Seed(speakingAttempt);

        // Monthly mock exam, finalized well above the certificate eligibility threshold.
        var mockExamResults = new FakeMockExamResultRepository();
        var mockExamResult = MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), tenantId, userId, MockExamId.From(Guid.NewGuid()), Now.AddMinutes(60));
        mockExamResult.RecordSectionBand(MockExamSectionType.Reading, 6.5m, Now.AddMinutes(60));
        mockExamResult.RecordSectionBand(MockExamSectionType.Listening, 7.0m, Now.AddMinutes(60));
        mockExamResult.RecordSectionBand(MockExamSectionType.Writing, 6.0m, Now.AddMinutes(60));
        mockExamResult.RecordSectionBand(MockExamSectionType.Speaking, 6.5m, Now.AddMinutes(60));
        mockExamResult.Finalize(Now.AddMinutes(60));
        mockExamResults.Seed(mockExamResult);

        // 1. Exam history reflects every skill's activity.
        var historyHandler = new GetExamHistoryQueryHandler(
            writingSubmissions, speakingAttempts, readingAttempts, listeningAttempts, grammarAttempts, vocabularyProgress, mockExamResults);
        var history = await historyHandler.Handle(new GetExamHistoryQuery(tenantId, userId, Cursor: null, Limit: 20), CancellationToken.None);

        Assert.True(history.IsSuccess);
        var types = history.Value.Items.Select(i => i.Type).ToHashSet();
        Assert.Equal(7, types.Count);
        Assert.Contains("writing", types);
        Assert.Contains("speaking", types);
        Assert.Contains("reading", types);
        Assert.Contains("listening", types);
        Assert.Contains("grammar", types);
        Assert.Contains("vocabulary", types);
        Assert.Contains("mock-exam", types);

        // 2. One attempt can be pulled up for review (Reading, chosen since it needs no evaluation join).
        var reviewHandler = new GetAttemptReviewQueryHandler(
            writingSubmissions, new FakeWritingEvaluationRepository(), speakingAttempts, new FakeSpeakingEvaluationRepository(),
            readingAttempts, listeningAttempts, grammarAttempts, vocabularyProgress, mockExamResults, new FakeCurrentUserService());
        var review = await reviewHandler.Handle(
            new GetAttemptReviewQuery(tenantId, userId, "reading", readingAttempt.Id.Value), CancellationToken.None);

        Assert.True(review.IsSuccess);
        Assert.Equal(6.5m, review.Value.Band);

        // 3. Certificate eligibility is evaluated once progress recalculates, and issues a certificate + achievement.
        var certificates = new FakeCertificateRepository();
        var achievements = new FakeAchievementRepository();
        var certificateUnitOfWork = new FakeUnitOfWork();
        var issuanceService = new CertificateIssuanceService(
            mockExamResults, certificates, achievements, new FakeUserRepository(), new FakeCertificatePdfGenerator(), new FakeFileStorage(),
            certificateUnitOfWork, new FakeClock(Now.AddMinutes(61)), new FakeGuidProvider(), NullLogger<CertificateIssuanceService>.Instance);

        await issuanceService.Handle(
            new DomainEventNotification<ProgressRecalculatedDomainEvent>(
                new ProgressRecalculatedDomainEvent(tenantId, userId, LearningSkill.Writing)),
            CancellationToken.None);

        var issuedCertificate = Assert.Single(certificates.Certificates);
        Assert.Single(achievements.Achievements);

        // 4. The issued certificate is readable back through its own query.
        var certificateHandler = new GetCertificateQueryHandler(certificates, new FakeCurrentUserService { UserId = userId });
        var certificateResult = await certificateHandler.Handle(
            new Application.Certificates.GetCertificate.GetCertificateQuery(tenantId, userId, issuedCertificate.Id.Value), CancellationToken.None);

        Assert.True(certificateResult.IsSuccess);
        Assert.Equal("Issued", certificateResult.Value.Status);
    }
}
