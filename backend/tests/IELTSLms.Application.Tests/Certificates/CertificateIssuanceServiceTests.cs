using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Certificates;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Application.Tests.Certificates.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Certificates;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Learning.Events;
using Microsoft.Extensions.Logging.Abstractions;
using FakeUnitOfWork = IELTSLms.Application.Tests.Grammar.Fakes.FakeUnitOfWork;
using FakeClock = IELTSLms.Application.Tests.Grammar.Fakes.FakeClock;
using FakeGuidProvider = IELTSLms.Application.Tests.Grammar.Fakes.FakeGuidProvider;

namespace IELTSLms.Application.Tests.Certificates;

public sealed class CertificateIssuanceServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static CertificateIssuanceService CreateService(
        FakeMockExamResultRepository mockExamResults, FakeCertificateRepository certificates, FakeAchievementRepository achievements,
        FakeFileStorage fileStorage, FakeUnitOfWork unitOfWork) =>
        new(mockExamResults, certificates, achievements, new FakeUserRepository(), new FakeCertificatePdfGenerator(), fileStorage,
            unitOfWork, new FakeClock(Now), new FakeGuidProvider(), NullLogger<CertificateIssuanceService>.Instance);

    private static MockExamResult SeedFinalizedResult(FakeMockExamResultRepository repo, Guid tenantId, Guid userId, decimal overallBand)
    {
        var result = MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), tenantId, userId, MockExamId.From(Guid.NewGuid()), Now);
        var band = overallBand;
        result.RecordSectionBand(MockExamSectionType.Reading, band, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, band, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, band, Now);
        result.RecordSectionBand(MockExamSectionType.Speaking, band, Now);
        result.Finalize(Now);
        repo.Seed(result);
        return result;
    }

    [Fact]
    public async Task Issues_a_certificate_and_awards_the_achievement_when_the_latest_result_is_eligible()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockExamResults = new FakeMockExamResultRepository();
        SeedFinalizedResult(mockExamResults, tenantId, userId, overallBand: 6.5m);
        var certificates = new FakeCertificateRepository();
        var achievements = new FakeAchievementRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(mockExamResults, certificates, achievements, new FakeFileStorage(), unitOfWork);

        await service.Handle(
            new DomainEventNotification<ProgressRecalculatedDomainEvent>(
                new ProgressRecalculatedDomainEvent(tenantId, userId, LearningSkill.Writing)),
            CancellationToken.None);

        var certificate = Assert.Single(certificates.Certificates);
        Assert.Equal(CertificateStatus.Issued, certificate.Status);
        Assert.NotNull(certificate.PdfStorageKey);
        Assert.Single(achievements.Achievements);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Does_nothing_when_the_latest_result_is_below_the_eligibility_threshold()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockExamResults = new FakeMockExamResultRepository();
        SeedFinalizedResult(mockExamResults, tenantId, userId, overallBand: 3.0m);
        var certificates = new FakeCertificateRepository();
        var service = CreateService(mockExamResults, certificates, new FakeAchievementRepository(), new FakeFileStorage(), new FakeUnitOfWork());

        await service.Handle(
            new DomainEventNotification<ProgressRecalculatedDomainEvent>(
                new ProgressRecalculatedDomainEvent(tenantId, userId, LearningSkill.Writing)),
            CancellationToken.None);

        Assert.Empty(certificates.Certificates);
    }

    [Fact]
    public async Task Does_nothing_when_the_learner_has_no_finalized_mock_exam_result()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var certificates = new FakeCertificateRepository();
        var service = CreateService(
            new FakeMockExamResultRepository(), certificates, new FakeAchievementRepository(), new FakeFileStorage(), new FakeUnitOfWork());

        await service.Handle(
            new DomainEventNotification<ProgressRecalculatedDomainEvent>(
                new ProgressRecalculatedDomainEvent(tenantId, userId, LearningSkill.Grammar)),
            CancellationToken.None);

        Assert.Empty(certificates.Certificates);
    }

    [Fact]
    public async Task Is_idempotent_a_second_ProgressRecalculated_for_the_same_result_does_not_issue_a_duplicate()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockExamResults = new FakeMockExamResultRepository();
        SeedFinalizedResult(mockExamResults, tenantId, userId, overallBand: 6.5m);
        var certificates = new FakeCertificateRepository();
        var achievements = new FakeAchievementRepository();
        var service = CreateService(mockExamResults, certificates, achievements, new FakeFileStorage(), new FakeUnitOfWork());
        var notification = new DomainEventNotification<ProgressRecalculatedDomainEvent>(
            new ProgressRecalculatedDomainEvent(tenantId, userId, LearningSkill.Writing));

        await service.Handle(notification, CancellationToken.None);
        await service.Handle(notification, CancellationToken.None);

        Assert.Single(certificates.Certificates);
        Assert.Single(achievements.Achievements);
    }
}
