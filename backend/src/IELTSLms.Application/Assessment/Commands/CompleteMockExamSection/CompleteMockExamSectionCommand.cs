using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;

namespace IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;

/// <summary>
/// Internal command invoked only by the event handlers that subscribe to each skill's own
/// scored/evaluated domain event (<c>ReadingAttemptScoredDomainEvent</c>,
/// <c>ListeningAttemptScoredDomainEvent</c>, <c>WritingEvaluatedDomainEvent</c>,
/// <c>SpeakingEvaluatedDomainEvent</c>) — never sent directly from an endpoint. This is the
/// domain-event-driven path DDD AR-3 requires for cross-aggregate consistency, as opposed to
/// <c>StartMockExamSection</c>'s direct command delegation at section-start time.
/// </summary>
public sealed record CompleteMockExamSectionCommand(Guid TenantId, Guid UserId, Guid MockExamId, MockExamSectionType Section, decimal Band)
    : ICommand;
