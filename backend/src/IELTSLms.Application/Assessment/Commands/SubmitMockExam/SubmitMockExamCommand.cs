using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Commands.SubmitMockExam;

/// <summary>
/// Signals exam-level completion. Normally the exam finalizes itself automatically the moment the
/// fourth section's scored/evaluated event arrives (<c>CompleteMockExamSection</c>); this is the
/// explicit client-triggered fallback for that same finalization — idempotent if already finalized,
/// and a clean business error if a section is still outstanding.
/// </summary>
public sealed record SubmitMockExamCommand(Guid TenantId, Guid UserId, Guid MockExamId) : ICommand<MockExamSubmitResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamSubmitResponse(Guid Id, string State, bool IsFinalized, decimal? OverallBand);
