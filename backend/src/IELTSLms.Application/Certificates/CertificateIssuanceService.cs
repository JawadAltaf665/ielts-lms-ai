using IELTSLms.Abstractions.Documents;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Certificates;
using IELTSLms.Domain.Identity;
using IELTSLms.Domain.Learning.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Certificates;

/// <summary>
/// Reacts to <see cref="ProgressRecalculatedDomainEvent"/> (POL-06: "whenever criteria are met, then
/// evaluate certificate eligibility") by checking the learner's latest finalized mock exam result
/// against <see cref="CertificateEligibilityService"/>, issuing a certificate + PDF + achievement when
/// eligible. Idempotent: re-firing for the same qualifying result is a safe no-op (checked via
/// <see cref="ICertificateRepository.GetBySourceResultAsync"/> before issuing) — every skill activity
/// raises <c>ProgressRecalculated</c>, so this handler runs far more often than it actually issues.
/// Fires from <c>ProgressTrackingService</c>'s own top-level save (nested post-commit dispatch, same
/// proven shape as <c>WritingEvaluatedHandler</c> → <c>ProgressTrackingService</c> itself).
/// </summary>
public sealed partial class CertificateIssuanceService(
    IMockExamResultRepository mockExamResults, ICertificateRepository certificates, IAchievementRepository achievements,
    IUserRepository users, ICertificatePdfGenerator pdfGenerator, IFileStorage fileStorage, IUnitOfWork unitOfWork,
    IClock clock, IGuidProvider guids, ILogger<CertificateIssuanceService> logger)
    : INotificationHandler<DomainEventNotification<ProgressRecalculatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<ProgressRecalculatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var latestResult = await mockExamResults
            .GetLatestFinalizedByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (latestResult is null || !CertificateEligibilityService.IsEligible(latestResult))
        {
            return;
        }

        var existing = await certificates
            .GetBySourceResultAsync(domainEvent.TenantId, CertificateSourceType.MockExamResult, latestResult.Id.Value, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return;
        }

        var now = clock.UtcNow;
        var verificationCode = GenerateVerificationCode();
        var certificate = Certificate.Issue(
            CertificateId.From(guids.NewGuid()), domainEvent.TenantId, domainEvent.UserId, CertificateSourceType.MockExamResult,
            latestResult.Id.Value, latestResult.OverallBand!.Value, verificationCode, now);

        var learner = await users.GetByIdAsync(UserId.From(domainEvent.UserId), cancellationToken).ConfigureAwait(false);
        var pdfBytes = pdfGenerator.Generate(new CertificatePdfModel(
            certificate.Id.Value, learner?.DisplayName ?? "IELTS Learner", certificate.OverallBand, verificationCode, now));

        var storageKey = $"certificates/{domainEvent.TenantId}/{certificate.Id.Value}.pdf";
        using (var pdfStream = new MemoryStream(pdfBytes))
        {
            await fileStorage.UploadAsync(storageKey, pdfStream, "application/pdf", cancellationToken).ConfigureAwait(false);
        }

        certificate.AttachPdf(storageKey, now);
        certificates.Add(certificate);

        if (!await achievements.ExistsAsync(domainEvent.TenantId, domainEvent.UserId, AchievementType.CertificateEarned, cancellationToken)
            .ConfigureAwait(false))
        {
            achievements.Add(Achievement.Award(
                AchievementId.From(guids.NewGuid()), domainEvent.TenantId, domainEvent.UserId, AchievementType.CertificateEarned, now));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        CertificateIssued(logger, certificate.Id.Value, domainEvent.UserId);
    }

    private static string GenerateVerificationCode() => $"IELTS-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

    [LoggerMessage(Level = LogLevel.Information, Message = "Issued certificate {CertificateId} for user {UserId}.")]
    private static partial void CertificateIssued(ILogger logger, Guid certificateId, Guid userId);
}
